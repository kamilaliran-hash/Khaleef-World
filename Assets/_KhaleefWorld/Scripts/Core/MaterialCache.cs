using System.Collections.Generic;
using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>One shared material per colour, cloned from the config's URP Lit material.</summary>
    public static class MaterialCache
    {
        static readonly Dictionary<int, Material> Cache = new Dictionary<int, Material>();

        public static Material Get(Color c)
        {
            Color32 c32 = c;
            int key = (c32.r << 24) | (c32.g << 16) | (c32.b << 8) | c32.a;
            if (Cache.TryGetValue(key, out var m) && m != null) return m;

            var cfg = GameConfig.Instance;
            Material baseMat = cfg != null ? cfg.blockyMaterial : null;
            if (baseMat != null) m = new Material(baseMat);
            else
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                m = new Material(shader);
            }
            m.color = c;
            m.enableInstancing = true;
            Cache[key] = m;
            return m;
        }
    }
}
