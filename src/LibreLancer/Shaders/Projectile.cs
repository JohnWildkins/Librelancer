﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LibreLancer.Shaders
{
    using System;
    
    public class Projectile
    {
        static ShaderVariables[] variants;
        private static bool iscompiled = false;
        public static ShaderVariables Get(ShaderFeatures features)
        {
            AllShaders.Compile();
            return variants[0];
        }
        public static ShaderVariables Get()
        {
            AllShaders.Compile();
            return variants[0];
        }
        internal static void Compile(string sourceBundle)
        {
            if (iscompiled)
            {
                return;
            }
            iscompiled = true;
            ShaderVariables.Log("Compiling Projectile");
            variants = new ShaderVariables[1];
            // No GL4 variants detected
            variants[0] = ShaderVariables.Compile(sourceBundle.Substring(337351, 466), sourceBundle.Substring(244700, 236));
        }
    }
}
