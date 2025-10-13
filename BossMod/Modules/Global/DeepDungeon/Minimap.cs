using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;

namespace BossMod.Global.DeepDungeon;

public sealed class Minimap(DeepDungeonState state, Angle playerRotation, int currentDestination, int playerSlot, ulong playerID)
{
    public readonly DeepDungeonState State = state;
    public readonly Angle PlayerRotation = playerRotation;
    public readonly int CurrentDestination = currentDestination;
    public readonly int PlayerSlot = playerSlot;
    public readonly ulong PlayerID = playerID;

    enum IconID : uint
    {
        ReturnClosed = 60905,
        ReturnOpen = 60906,
        PassageClosed = 60907,
        PassageOpen = 60908,
        ChestBronze = 60911,
        ChestSilver = 60912,
        ChestGold = 60913,
        Votive = 63988
    }

    [Flags]
    enum RoomChest
    {
        None = 0,
        Bronze = 1,
        Silver = 2,
        Gold = 4
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Integer index of the room the user clicked on.</returns>
    public int Draw()
    {
        var dest = -1;

        var chests = new RoomChest[25];
        var lenC = State.Chests.Length;

        for (var i = 0; i < lenC; ++i)
        {
            ref readonly var c = ref State.Chests[i];
            if (c.Room > 0)
                chests[c.Room] |= (RoomChest)(1 << (c.Type - 1));
        }

        var lenP = State.Party.Length;
        DeepDungeonState.PartyMember player = default;
        for (var i = 0; i < lenP; ++i)
        {
            ref readonly var p = ref State.Party[i];
            if (p.EntityId == PlayerID)
            {
                player = p;
                break;
            }
        }
        var playerCell = player.Room;

        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, (Vector2)default);

        var roomsTex = Service.Texture.GetFromGame("ui/uld/DeepDungeonNaviMap_Rooms_hr1.tex").GetWrapOrEmpty();
        var mapTex = Service.Texture.GetFromGame("ui/uld/DeepDungeonNaviMap_hr1.tex").GetWrapOrEmpty();
        var passageTex = Service.Texture.GetFromGameIcon(new((uint)(State.PassageActive ? IconID.PassageOpen : IconID.PassageClosed))).GetWrapOrEmpty();
        var returnTex = Service.Texture.GetFromGameIcon(new((uint)(State.ReturnActive ? IconID.ReturnOpen : IconID.ReturnClosed))).GetWrapOrEmpty();
        var votiveTex = Service.Texture.GetFromGameIcon(new((uint)IconID.Votive)).GetWrapOrEmpty();
        var bronzeTex = Service.Texture.GetFromGameIcon(new((uint)IconID.ChestBronze)).GetWrapOrEmpty();
        var silverTex = Service.Texture.GetFromGameIcon(new((uint)IconID.ChestSilver)).GetWrapOrEmpty();
        var goldTex = Service.Texture.GetFromGameIcon(new((uint)IconID.ChestGold)).GetWrapOrEmpty();

        for (var i = 0; i < 25; ++i)
        {
            var highlight = CurrentDestination > 0 && CurrentDestination == i;

            var isValidDestination = State.Rooms[i] > 0;

            using var _1 = ImRaii.PushId($"room{i}");

            var pos = ImGui.GetCursorPos();
            var tile = (byte)State.Rooms[i] & 0xF;
            var row = tile / 4;
            var col = tile & 3;

            var xoff = 0.0104f + col * 0.25f;
            var yoff = 0.0104f + row * 0.25f;
            var xoffend = xoff + 0.2292f;
            var yoffend = yoff + 0.2292f;

            // trim off 1px from each edge to account for extra space from highlight square
            // TODO there is probably a sensible primitive for this somewhere
            if (highlight)
            {
                xoff += 0.2292f / 88f;
                yoff += 0.2292f / 88f;
                xoffend -= 0.2292f / 88f;
                yoffend -= 0.2292f / 88f;
            }

            ImGui.SetCursorPos(pos);
            ImGui.Image(roomsTex.Handle, highlight ? new(86) : new(88), new Vector2(xoff, yoff), new Vector2(xoffend, yoffend), tile > 0 ? new(1f) : new(0.6f), highlight ? new(0f, 0.6f, 0f, 1f) : default);

            if (i == playerCell)
            {
                isValidDestination = false;
                ImGui.SetCursorPos(pos + new Vector2(12f, 12f));
                ImGui.Image(mapTex.Handle, new Vector2(64f, 64f), new Vector2(0.2424f, 0.4571f), new Vector2(0.4848f, 0.6857f));
            }

            if (State.Rooms[i].HasFlag(RoomFlags.Passage))
            {
                ImGui.SetCursorPos(pos + new Vector2(28f, 44f));
                ImGui.Image(passageTex.Handle, new Vector2(32f, 32f));
            }

            if (State.Rooms[i].HasFlag(RoomFlags.Return))
            {
                ImGui.SetCursorPos(pos + new Vector2(28f, 44f));
                ImGui.Image(returnTex.Handle, new Vector2(32f, 32f));
            }

            if (((ushort)State.Rooms[i] & 0x100) != 0)
            {
                ImGui.SetCursorPos(pos + new Vector2(28f, 44f));
                ImGui.Image(votiveTex.Handle, new Vector2(32f, 32f));
            }

            if (chests[i].HasFlag(RoomChest.Bronze))
            {
                ImGui.SetCursorPos(pos + new Vector2(2f, 2f));
                ImGui.Image(bronzeTex.Handle, new Vector2(48f, 48f));
            }

            if (chests[i].HasFlag(RoomChest.Silver))
            {
                ImGui.SetCursorPos(pos + new Vector2(20f, 2f));
                ImGui.Image(silverTex.Handle, new Vector2(48f, 48f));
            }

            if (chests[i].HasFlag(RoomChest.Gold))
            {
                ImGui.SetCursorPos(pos + new Vector2(38f, 2f));
                ImGui.Image(goldTex.Handle, new Vector2(48f, 48f));
            }

            if (i == playerCell)
            {
                ImGui.SetCursorPos(pos + new Vector2(44f, 44f));
                DrawPlayer(ImGui.GetCursorScreenPos(), PlayerRotation, mapTex.Handle);
            }

            ImGui.SetCursorPos(pos);
            ImGui.Dummy(new(88f, 88f));
            if (isValidDestination)
            {
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    ImGui.SetTooltip(i == CurrentDestination ? "Click to clear destination" : "Click to set destination");
                }
                if (ImGui.IsItemClicked())
                    dest = i == CurrentDestination ? 0 : i;
            }
            if (i % 5 < 4)
                ImGui.SameLine();
        }

        return dest;
    }

    private static void DrawPlayer(Vector2 center, Angle rotation, ImTextureID texHandle)
    {
        var cos = -rotation.Cos();
        var sin = rotation.Sin();
        ImGui.GetWindowDrawList().AddImageQuad(
            texHandle,
            center + Rotate(new(-32f, -37.5f), cos, sin),
            center + Rotate(new(32f, -37.5f), cos, sin),
            center + Rotate(new(32f, 26.5f), cos, sin),
            center + Rotate(new(-32f, 26.5f), cos, sin),
            new Vector2(0.0000f, 0.4571f),
            new Vector2(0.2424f, 0.4571f),
            new Vector2(0.2424f, 0.6857f),
            new Vector2(0.0000f, 0.6857f)
        );
    }

    private static Vector2 Rotate(Vector2 v, float cosA, float sinA) => new(v.X * cosA - v.Y * sinA, v.X * sinA + v.Y * cosA);
}
