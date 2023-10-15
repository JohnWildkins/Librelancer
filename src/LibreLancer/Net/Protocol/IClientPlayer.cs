using System.Numerics;
using LibreLancer.World;

namespace LibreLancer.Net.Protocol;

[RPCInterface]
public interface IClientPlayer
{
    void UpdateBaselinePrices(BaselinePrice[] prices);
    void CallThorn(string script, ObjNetId mainObject);
    void SpawnObject(int id, ObjectName name, string affiliation, Vector3 position, Quaternion orientation, NetShipLoadout loadout);
    void SpawnPlayer(int id, string system, NetObjective objective, Vector3 position, Quaternion orientation, uint tick);
    void UpdateEffects(ObjNetId id, SpawnedEffect[] effects);
    void SpawnProjectiles(ProjectileSpawn[] projectiles);
    void UpdateAnimations(ObjNetId id, NetCmpAnimation[] animations);
    void UpdateReputations(NetReputation[] reps);
    void UpdateInventory(long credits, ulong shipworth, NetShipLoadout ship);
    void UpdateSlotCount(int slot, int count);
    void DeleteSlot(int slot);
    void SpawnSolar(SolarInfo[] solars);
    void OnConsoleMessage(string text);
    void SpawnDebris(int id, GameObjectKind kind, string archetype, string part, Vector3 position, Quaternion orientation, float mass);
    void SpawnMissile(int id, bool playSound, uint equip, Vector3 position, Quaternion orientation);
    void DestroyMissile(int id, bool explode);
    void BaseEnter(string _base, NetObjective objective, string[] rtcs, NewsArticle[] news, SoldGood[] goods, NetSoldShip[] ships);
    void UpdateRTCs(string[] rtcs);
    void SetObjective(NetObjective objective);
    void Killed();
    void DespawnObject(int id, bool explode);
    void PlaySound(string sound);
    void PlayMusic(string music, float fade);
    void DestroyPart(ObjNetId id, string part);
    void RunMissionDialog(NetDlgLine[] lines);
    void StartJumpTunnel();
    void StartTradelane();
    void TradelaneDisrupted();
    void EndTradelane();
    void UpdateFormation(NetFormation formation);
    void TradelaneActivate(uint id, bool left);
    void TradelaneDeactivate(uint id, bool left);
    void MarkImportant(int objId);
    void ReceiveChatMessage(ChatCategory category, string player, string message);
    void PopupOpen(int title, int contents, string id);
    void OnPlayerJoin(int id, string name);
    void OnPlayerLeave(int id, string name);
    void StopShip();
}
