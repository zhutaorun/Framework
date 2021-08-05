using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameFrame.UI
{
    internal class MaterialInfo
    {
        public MaterialInfo(Material mat)
        {
            if(null==mat)
            {
                throw new ArgumentNullException();
            }

            _material = mat;
        }

        public void Attach(Graphic target)
        {
            if(null== target)
            {
                return;
            }
            target.material = _material;
            _observers[target] = this;
        }

        public void Detach(Graphic target)
        {
            if(null==target)
            {
                return;
            }
            _observers.Remove(target);
        }

        public Material GetMaterial()
        {
            return _material;
        }

        public int GetCount()
        {
            return _observers.Count;
        }
        private Material _material;
        private readonly Hashtable _observers = new Hashtable();
    }

}