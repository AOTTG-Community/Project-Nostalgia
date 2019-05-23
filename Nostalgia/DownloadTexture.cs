using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(UITexture))]
public class DownloadTexture : MonoBehaviour
{
    private Material mMat;
    private Texture2D mTex;
    public string url = "http://www.tasharen.com/misc/logo.png";

    private void OnDestroy()
    {
        if (this.mMat != null)
        {
            UnityEngine.Object.Destroy(this.mMat);
        }
        if (this.mTex != null)
        {
            UnityEngine.Object.Destroy(this.mTex);
        }
    }

    [DebuggerHidden]
    private IEnumerator Start()
    {
        return new c__Iterator7 { f__this = this };
    }

    [CompilerGenerated]
    private sealed class c__Iterator7 : IEnumerator, IDisposable, IEnumerator<object>
    {
        internal object current;
        internal int PC;
        internal DownloadTexture f__this;
        internal UITexture ut__1;
        internal WWW www__0;

        [DebuggerHidden]
        public void Dispose()
        {
            this.PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.PC;
            this.PC = -1;
            switch (num)
            {
                case 0:
                    this.www__0 = new WWW(this.f__this.url);
                    this.current = this.www__0;
                    this.PC = 1;
                    return true;

                case 1:
                    this.f__this.mTex = this.www__0.texture;
                    if (this.f__this.mTex == null)
                    {
                        goto Label_0118;
                    }
                    this.ut__1 = this.f__this.GetComponent<UITexture>();
                    if (this.ut__1.material != null)
                    {
                        this.f__this.mMat = new Material(this.ut__1.material);
                        break;
                    }
                    this.f__this.mMat = new Material(Shader.Find("Unlit/Transparent Colored"));
                    break;

                default:
                    goto Label_012A;
            }
            this.ut__1.material = this.f__this.mMat;
            this.f__this.mMat.mainTexture = this.f__this.mTex;
            this.ut__1.MakePixelPerfect();
        Label_0118:
            this.www__0.Dispose();
            this.PC = -1;
        Label_012A:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.current;
            }
        }
    }
}

