﻿/* The contents of this file are subject to the Mozilla Public License
 * Version 1.1 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS"
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
 * License for the specific language governing rights and limitations
 * under the License.
 * 
 * 
 * The Initial Developer of the Original Code is Callum McGing (mailto:callum.mcging@gmail.com).
 * Portions created by the Initial Developer are Copyright (C) 2013-2016
 * the Initial Developer. All Rights Reserved.
 */
using System;
using System.Threading;

namespace LibreLancer.GameData
{
	public class ZoneEllipsoid : ZoneShape
	{
		public Vector3 Size;
		static readonly ThreadLocal<Vector3[]> cornerbuf = new ThreadLocal<Vector3[]>(() => new Vector3[6]);
		public ZoneEllipsoid (float x, float y, float z)
		{
			Size = new Vector3 (x, y, z);
		}
		public override bool Intersects(Vector3 position, BoundingBox box)
		{
			var corners = cornerbuf.Value;
			box.GetCorners(corners);
			foreach (var c in corners)
			{
				if (PrimitiveMath.EllipsoidContains(position, Size, c))
					return true;
			}
			return false;
		}
		public override bool ContainsPoint(Vector3 position, Matrix4 rotation, Vector3 point)
		{
			//Transform point
			var R = rotation;
			R.Transpose();
			point = R.Transform(point) - R.Transform(position);
			//Test
			return PrimitiveMath.EllipsoidContains(Vector3.Zero, Size, point);
		}
		public override ZoneShape Scale(float scale)
		{
			var scl = Size * scale;
			return new ZoneEllipsoid(scl.X, scl.Y, scl.Z);
		}
		public override float ScaledDistance(Vector3 position, Vector3 point)
		{
			return PrimitiveMath.EllipsoidFunction(position, Size, point);
		}
		public override Vector3 RandomPoint (Func<float> randfunc)
		{
			var theta = randfunc () * 2 * Math.PI;
			var phi = randfunc () * 2 * Math.PI;
			var x = Math.Cos (theta) * Math.Cos (phi);
			var y = Math.Sin (phi);
			var z = Math.Sin (theta) * Math.Cos (phi);
			return new Vector3 ((float)x, (float)y, (float)z) * Size;
		}
	}
}

