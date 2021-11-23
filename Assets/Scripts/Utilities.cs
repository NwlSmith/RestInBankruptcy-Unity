using UnityEngine;

public static class Utilities
{
    
    
    public static bool TryGetComponentInChildren<T>(this Component c, out T retVal) where T : Component {
        foreach(Transform t in c.transform) {
            if(t.TryGetComponent<T>(out retVal)) return true;
        }
        retVal = null;
        return false;
    }
    
    
    
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}
