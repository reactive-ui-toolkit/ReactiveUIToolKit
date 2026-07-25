using System;
using System.Collections.Generic;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Ugui
{
    /// <summary>
    /// Base class for all uGUI host-element props. Prop names mirror the
    /// RectTransform Inspector one-to-one; nullable means "not set — leave the
    /// component's current value". No UI Toolkit Style surface exists here by
    /// design: uGUI elements are positioned with anchors/pivots and styled
    /// with sprites, colors, and materials.
    /// </summary>
    public abstract class UguiBaseProps : HostPropsBase
    {
        static UguiBaseProps()
        {
            HostPropsBase.__RegisterFamilyFlusher(__FlushReturns);
        }

        // --- RectTransform (anchors preset first, explicit values override) ---
        public UguiAnchorPreset? Anchors { get; set; }
        public Vector2? AnchorMin { get; set; }
        public Vector2? AnchorMax { get; set; }
        public Vector2? Pivot { get; set; }
        public Vector2? AnchoredPosition { get; set; }
        public Vector2? SizeDelta { get; set; }
        public Vector2? OffsetMin { get; set; }
        public Vector2? OffsetMax { get; set; }
        public float? RotationZ { get; set; }
        public Vector3? Scale { get; set; }

        // --- GameObject ---
        public bool? Active { get; set; }
        public int? Layer { get; set; }

        public virtual bool ShallowEquals(UguiBaseProps other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (GetType() != other.GetType())
                return false;

            if (Name != other.Name)
                return false;
            if (!ReferenceEquals(Ref, other.Ref))
                return false;
            if (Anchors != other.Anchors)
                return false;
            if (AnchorMin != other.AnchorMin)
                return false;
            if (AnchorMax != other.AnchorMax)
                return false;
            if (Pivot != other.Pivot)
                return false;
            if (AnchoredPosition != other.AnchoredPosition)
                return false;
            if (SizeDelta != other.SizeDelta)
                return false;
            if (OffsetMin != other.OffsetMin)
                return false;
            if (OffsetMax != other.OffsetMax)
                return false;
            if (RotationZ != other.RotationZ)
                return false;
            if (Scale != other.Scale)
                return false;
            if (Active != other.Active)
                return false;
            if (Layer != other.Layer)
                return false;

            return true;
        }

        public override bool HostShallowEquals(HostPropsBase other)
        {
            return other is UguiBaseProps up && ShallowEquals(up);
        }

        internal override void __ScheduleReturnToFamilyPool()
        {
            __ScheduleReturn(this);
        }

        internal void __ResetBase()
        {
            Name = null;
            Ref = null;
            Anchors = null;
            AnchorMin = null;
            AnchorMax = null;
            Pivot = null;
            AnchoredPosition = null;
            SizeDelta = null;
            OffsetMin = null;
            OffsetMax = null;
            RotationZ = null;
            Scale = null;
            Active = null;
            Layer = null;
        }

        internal virtual void __ResetFields() { }

        internal virtual void __ReturnToPool() { }

        // ═══════════════════════════════════════════════════════════════════
        //  Family pool — mirrors the BaseProps pool shape exactly
        // ═══════════════════════════════════════════════════════════════════

        internal static class Pool<T>
            where T : UguiBaseProps, new()
        {
            private const int Capacity = 4096;

            private static readonly Stack<T> s_pool = new Stack<T>(256);
            private static uint s_nextGeneration = 1;

            internal static T Rent()
            {
                T p;
                if (s_pool.Count > 0)
                {
                    p = s_pool.Pop();
                    p.__ResetBase();
                    p.__ResetFields();
                }
                else
                {
                    p = new T();
                }
                uint gen = s_nextGeneration++;
                if (gen == 0)
                    gen = s_nextGeneration++;
                p._generation = gen;
                p._isPendingReturn = false;
                return p;
            }

            internal static void Return(T p)
            {
                if (s_pool.Count < Capacity)
                    s_pool.Push(p);
            }
        }

        public static T __Rent<T>()
            where T : UguiBaseProps, new()
        {
            return Pool<T>.Rent();
        }

        private static readonly List<UguiBaseProps> s_pendingReturn =
            new List<UguiBaseProps>(2048);

        internal static void __ScheduleReturn(UguiBaseProps p)
        {
            if (p == null || p._generation == 0)
                return;
            if (p._isPendingReturn)
                return;
            p._isPendingReturn = true;
            s_pendingReturn.Add(p);
        }

        internal static void __FlushReturns()
        {
            for (int i = 0; i < s_pendingReturn.Count; i++)
            {
                var p = s_pendingReturn[i];
                p._isPendingReturn = false;
                p.__ReturnToPool();
            }
            s_pendingReturn.Clear();
        }
    }
}
