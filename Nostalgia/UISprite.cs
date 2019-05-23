using System;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Sprite"), ExecuteInEditMode]
public class UISprite : UIWidget
{
    [HideInInspector, SerializeField]
    private UIAtlas mAtlas;
    [HideInInspector, SerializeField]
    private float mFillAmount = 1f;
    [SerializeField, HideInInspector]
    private bool mFillCenter = true;
    [SerializeField, HideInInspector]
    private FillDirection mFillDirection = FillDirection.Radial360;
    protected Rect mInner;
    protected Rect mInnerUV;
    [HideInInspector, SerializeField]
    private bool mInvert;
    protected Rect mOuter;
    protected Rect mOuterUV;
    protected Vector3 mScale = Vector3.one;
    protected UIAtlas.Sprite mSprite;
    [SerializeField, HideInInspector]
    private string mSpriteName;
    private bool mSpriteSet;
    [SerializeField, HideInInspector]
    private Type mType;

    protected bool AdjustRadial(Vector2[] xy, Vector2[] uv, float fill, bool invert)
    {
        if (fill < 0.001f)
        {
            return false;
        }
        if (invert || (fill <= 0.999f))
        {
            float f = Mathf.Clamp01(fill);
            if (!invert)
            {
                f = 1f - f;
            }
            f *= 1.570796f;
            float t = Mathf.Sin(f);
            float num3 = Mathf.Cos(f);
            if (t > num3)
            {
                num3 *= 1f / t;
                t = 1f;
                if (!invert)
                {
                    xy[0].y = Mathf.Lerp(xy[2].y, xy[0].y, num3);
                    xy[3].y = xy[0].y;
                    uv[0].y = Mathf.Lerp(uv[2].y, uv[0].y, num3);
                    uv[3].y = uv[0].y;
                }
            }
            else if (num3 > t)
            {
                t *= 1f / num3;
                num3 = 1f;
                if (invert)
                {
                    xy[0].x = Mathf.Lerp(xy[2].x, xy[0].x, t);
                    xy[1].x = xy[0].x;
                    uv[0].x = Mathf.Lerp(uv[2].x, uv[0].x, t);
                    uv[1].x = uv[0].x;
                }
            }
            else
            {
                t = 1f;
                num3 = 1f;
            }
            if (invert)
            {
                xy[1].y = Mathf.Lerp(xy[2].y, xy[0].y, num3);
                uv[1].y = Mathf.Lerp(uv[2].y, uv[0].y, num3);
            }
            else
            {
                xy[3].x = Mathf.Lerp(xy[2].x, xy[0].x, t);
                uv[3].x = Mathf.Lerp(uv[2].x, uv[0].x, t);
            }
        }
        return true;
    }

    protected void FilledFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		float x0 = 0f;
		float y0 = 0f;
		float x1 = 1f;
		float y1 = -1f;

		float u0 = mOuterUV.xMin;
		float v0 = mOuterUV.yMin;
		float u1 = mOuterUV.xMax;
		float v1 = mOuterUV.yMax;

		// Horizontal and vertical filled sprites are simple -- just end the sprite prematurely
		if (mFillDirection == FillDirection.Horizontal || mFillDirection == FillDirection.Vertical)
		{
			float du = (u1 - u0) * mFillAmount;
			float dv = (v1 - v0) * mFillAmount;

			if (fillDirection == FillDirection.Horizontal)
			{
				if (mInvert)
				{
					x0 = (1f - mFillAmount);
					u0 = u1 - du;
				}
				else
				{
					x1 *= mFillAmount;
					u1 = u0 + du;
				}
			}
			else if (fillDirection == FillDirection.Vertical)
			{
				if (mInvert)
				{
					y1 *= mFillAmount;
					v0 = v1 - dv;
				}
				else
				{
					y0 = -(1f - mFillAmount);
					v1 = v0 + dv;
				}
			}
		}

		// Starting quad for the sprite
		Vector2[] xy = new Vector2[4];
		Vector2[] uv = new Vector2[4];

		xy[0] = new Vector2(x1, y0);
		xy[1] = new Vector2(x1, y1);
		xy[2] = new Vector2(x0, y1);
		xy[3] = new Vector2(x0, y0);

		uv[0] = new Vector2(u1, v1);
		uv[1] = new Vector2(u1, v0);
		uv[2] = new Vector2(u0, v0);
		uv[3] = new Vector2(u0, v1);

		Color colF = color;
		colF.a *= mPanel.alpha;
		Color32 col = atlas.premultipliedAlpha ? NGUITools.ApplyPMA(colF) : colF;

		if (fillDirection == FillDirection.Radial90)
		{
			// Adjust the quad radially, and if 'false' is returned (it's not visible), just exit
			if (!AdjustRadial(xy, uv, mFillAmount, mInvert)) return;
		}
		else if (fillDirection == FillDirection.Radial180)
		{
			// Working in 0-1 coordinates is easier
			Vector2[] oxy = new Vector2[4];
			Vector2[] ouv = new Vector2[4];

			for (int i = 0; i < 2; ++i)
			{
				oxy[0] = new Vector2(0f, 0f);
				oxy[1] = new Vector2(0f, 1f);
				oxy[2] = new Vector2(1f, 1f);
				oxy[3] = new Vector2(1f, 0f);

				ouv[0] = new Vector2(0f, 0f);
				ouv[1] = new Vector2(0f, 1f);
				ouv[2] = new Vector2(1f, 1f);
				ouv[3] = new Vector2(1f, 0f);

				// Each half must be rotated 90 degrees clockwise in order for it to fill properly
				if (mInvert)
				{
					if (i > 0)
					{
						Rotate(oxy, i);
						Rotate(ouv, i);
					}
				}
				else if (i < 1)
				{
					Rotate(oxy, 1 - i);
					Rotate(ouv, 1 - i);
				}

				// Each half must fill in only a part of the space
				float x, y;

				if (i == 1)
				{
					x = mInvert ? 0.5f : 1f;
					y = mInvert ? 1f : 0.5f;
				}
				else
				{
					x = mInvert ? 1f : 0.5f;
					y = mInvert ? 0.5f : 1f;
				}

				oxy[1].y = Mathf.Lerp(x, y, oxy[1].y);
				oxy[2].y = Mathf.Lerp(x, y, oxy[2].y);
				ouv[1].y = Mathf.Lerp(x, y, ouv[1].y);
				ouv[2].y = Mathf.Lerp(x, y, ouv[2].y);

				float amount = (mFillAmount) * 2 - i;
				bool odd = (i % 2) == 1;

				if (AdjustRadial(oxy, ouv, amount, !odd))
				{
					if (mInvert) odd = !odd;

					// Add every other side in reverse order so they don't come out backface-culled due to rotation
					if (odd)
					{
						for (int b = 0; b < 4; ++b)
						{
							x = Mathf.Lerp(xy[0].x, xy[2].x, oxy[b].x);
							y = Mathf.Lerp(xy[0].y, xy[2].y, oxy[b].y);

							float u = Mathf.Lerp(uv[0].x, uv[2].x, ouv[b].x);
							float v = Mathf.Lerp(uv[0].y, uv[2].y, ouv[b].y);

							verts.Add(new Vector3(x, y, 0f));
							uvs.Add(new Vector2(u, v));
							cols.Add(col);
						}
					}
					else
					{
						for (int b = 3; b > -1; --b)
						{
							x = Mathf.Lerp(xy[0].x, xy[2].x, oxy[b].x);
							y = Mathf.Lerp(xy[0].y, xy[2].y, oxy[b].y);

							float u = Mathf.Lerp(uv[0].x, uv[2].x, ouv[b].x);
							float v = Mathf.Lerp(uv[0].y, uv[2].y, ouv[b].y);

							verts.Add(new Vector3(x, y, 0f));
							uvs.Add(new Vector2(u, v));
							cols.Add(col);
						}
					}
				}
			}
			return;
		}
		else if (fillDirection == FillDirection.Radial360)
		{
			float[] matrix = new float[]
			{
				// x0 y0  x1   y1
				0.5f, 1f, 0f, 0.5f, // quadrant 0
				0.5f, 1f, 0.5f, 1f, // quadrant 1
				0f, 0.5f, 0.5f, 1f, // quadrant 2
				0f, 0.5f, 0f, 0.5f, // quadrant 3
			};

			Vector2[] oxy = new Vector2[4];
			Vector2[] ouv = new Vector2[4];

			for (int i = 0; i < 4; ++i)
			{
				oxy[0] = new Vector2(0f, 0f);
				oxy[1] = new Vector2(0f, 1f);
				oxy[2] = new Vector2(1f, 1f);
				oxy[3] = new Vector2(1f, 0f);

				ouv[0] = new Vector2(0f, 0f);
				ouv[1] = new Vector2(0f, 1f);
				ouv[2] = new Vector2(1f, 1f);
				ouv[3] = new Vector2(1f, 0f);

				// Each quadrant must be rotated 90 degrees clockwise in order for it to fill properly
				if (mInvert)
				{
					if (i > 0)
					{
						Rotate(oxy, i);
						Rotate(ouv, i);
					}
				}
				else if (i < 3)
				{
					Rotate(oxy, 3 - i);
					Rotate(ouv, 3 - i);
				}

				// Each quadrant must fill in only a quarter of the space
				for (int b = 0; b < 4; ++b)
				{
					int index = (mInvert) ? (3 - i) * 4 : i * 4;

					float fx0 = matrix[index];
					float fy0 = matrix[index + 1];
					float fx1 = matrix[index + 2];
					float fy1 = matrix[index + 3];

					oxy[b].x = Mathf.Lerp(fx0, fy0, oxy[b].x);
					oxy[b].y = Mathf.Lerp(fx1, fy1, oxy[b].y);
					ouv[b].x = Mathf.Lerp(fx0, fy0, ouv[b].x);
					ouv[b].y = Mathf.Lerp(fx1, fy1, ouv[b].y);
				}

				float amount = (mFillAmount) * 4 - i;
				bool odd = (i % 2) == 1;

				if (AdjustRadial(oxy, ouv, amount, !odd))
				{
					if (mInvert) odd = !odd;

					// Add every other side in reverse order so they don't come out backface-culled due to rotation
					if (odd)
					{
						for (int b = 0; b < 4; ++b)
						{
							float x = Mathf.Lerp(xy[0].x, xy[2].x, oxy[b].x);
							float y = Mathf.Lerp(xy[0].y, xy[2].y, oxy[b].y);
							float u = Mathf.Lerp(uv[0].x, uv[2].x, ouv[b].x);
							float v = Mathf.Lerp(uv[0].y, uv[2].y, ouv[b].y);

							verts.Add(new Vector3(x, y, 0f));
							uvs.Add(new Vector2(u, v));
							cols.Add(col);
						}
					}
					else
					{
						for (int b = 3; b > -1; --b)
						{
							float x = Mathf.Lerp(xy[0].x, xy[2].x, oxy[b].x);
							float y = Mathf.Lerp(xy[0].y, xy[2].y, oxy[b].y);
							float u = Mathf.Lerp(uv[0].x, uv[2].x, ouv[b].x);
							float v = Mathf.Lerp(uv[0].y, uv[2].y, ouv[b].y);

							verts.Add(new Vector3(x, y, 0f));
							uvs.Add(new Vector2(u, v));
							cols.Add(col);
						}
					}
				}
			}
			return;
		}

		// Fill the buffer with the quad for the sprite
		for (int i = 0; i < 4; ++i)
		{
			verts.Add(xy[i]);
			uvs.Add(uv[i]);
			cols.Add(col);
		}
	}

    public UIAtlas.Sprite GetAtlasSprite()
    {
        if (!this.mSpriteSet)
        {
            this.mSprite = null;
        }
        if ((this.mSprite == null) && (this.mAtlas != null))
        {
            if (!string.IsNullOrEmpty(this.mSpriteName))
            {
                UIAtlas.Sprite sp = this.mAtlas.GetSprite(this.mSpriteName);
                if (sp == null)
                {
                    return null;
                }
                this.SetAtlasSprite(sp);
            }
            if ((this.mSprite == null) && (this.mAtlas.spriteList.Count > 0))
            {
                UIAtlas.Sprite sprite2 = this.mAtlas.spriteList[0];
                if (sprite2 == null)
                {
                    return null;
                }
                this.SetAtlasSprite(sprite2);
                if (this.mSprite == null)
                {
                    Debug.LogError(this.mAtlas.name + " seems to have a null sprite!");
                    return null;
                }
                this.mSpriteName = this.mSprite.name;
            }
            if (this.mSprite != null)
            {
                this.material = this.mAtlas.spriteMaterial;
                this.UpdateUVs(true);
            }
        }
        return this.mSprite;
    }

    public override void MakePixelPerfect()
    {
        if (this.isValid)
        {
            this.UpdateUVs(false);
            switch (this.type)
            {
                case Type.Sliced:
                {
                    Vector3 localPosition = base.cachedTransform.localPosition;
                    localPosition.x = Mathf.RoundToInt(localPosition.x);
                    localPosition.y = Mathf.RoundToInt(localPosition.y);
                    localPosition.z = Mathf.RoundToInt(localPosition.z);
                    base.cachedTransform.localPosition = localPosition;
                    Vector3 localScale = base.cachedTransform.localScale;
                    localScale.x = Mathf.RoundToInt(localScale.x * 0.5f) << 1;
                    localScale.y = Mathf.RoundToInt(localScale.y * 0.5f) << 1;
                    localScale.z = 1f;
                    base.cachedTransform.localScale = localScale;
                    break;
                }
                case Type.Tiled:
                {
                    Vector3 vector3 = base.cachedTransform.localPosition;
                    vector3.x = Mathf.RoundToInt(vector3.x);
                    vector3.y = Mathf.RoundToInt(vector3.y);
                    vector3.z = Mathf.RoundToInt(vector3.z);
                    base.cachedTransform.localPosition = vector3;
                    Vector3 vector4 = base.cachedTransform.localScale;
                    vector4.x = Mathf.RoundToInt(vector4.x);
                    vector4.y = Mathf.RoundToInt(vector4.y);
                    vector4.z = 1f;
                    base.cachedTransform.localScale = vector4;
                    break;
                }
                default:
                {
                    Texture mainTexture = this.mainTexture;
                    Vector3 vector5 = base.cachedTransform.localScale;
                    if (mainTexture != null)
                    {
                        Rect rect = NGUIMath.ConvertToPixels(this.outerUV, mainTexture.width, mainTexture.height, true);
                        float pixelSize = this.atlas.pixelSize;
                        vector5.x = Mathf.RoundToInt(rect.width * pixelSize) * Mathf.Sign(vector5.x);
                        vector5.y = Mathf.RoundToInt(rect.height * pixelSize) * Mathf.Sign(vector5.y);
                        vector5.z = 1f;
                        base.cachedTransform.localScale = vector5;
                    }
                    int num2 = Mathf.RoundToInt(Mathf.Abs(vector5.x) * ((1f + this.mSprite.paddingLeft) + this.mSprite.paddingRight));
                    int num3 = Mathf.RoundToInt(Mathf.Abs(vector5.y) * ((1f + this.mSprite.paddingTop) + this.mSprite.paddingBottom));
                    Vector3 vector6 = base.cachedTransform.localPosition;
                    vector6.x = Mathf.CeilToInt(vector6.x * 4f) >> 2;
                    vector6.y = Mathf.CeilToInt(vector6.y * 4f) >> 2;
                    vector6.z = Mathf.RoundToInt(vector6.z);
                    if (((num2 % 2) == 1) && (((base.pivot == UIWidget.Pivot.Top) || (base.pivot == UIWidget.Pivot.Center)) || (base.pivot == UIWidget.Pivot.Bottom)))
                    {
                        vector6.x += 0.5f;
                    }
                    if (((num3 % 2) == 1) && (((base.pivot == UIWidget.Pivot.Left) || (base.pivot == UIWidget.Pivot.Center)) || (base.pivot == UIWidget.Pivot.Right)))
                    {
                        vector6.y += 0.5f;
                    }
                    base.cachedTransform.localPosition = vector6;
                    break;
                }
            }
        }
    }

    public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
    {
        switch (this.type)
        {
            case Type.Simple:
                this.SimpleFill(verts, uvs, cols);
                break;

            case Type.Sliced:
                this.SlicedFill(verts, uvs, cols);
                break;

            case Type.Tiled:
                this.TiledFill(verts, uvs, cols);
                break;

            case Type.Filled:
                this.FilledFill(verts, uvs, cols);
                break;
        }
    }

    protected override void OnStart()
    {
        if (this.mAtlas != null)
        {
            this.UpdateUVs(true);
        }
    }

    protected void Rotate(Vector2[] v, int offset)
    {
        for (int i = 0; i < offset; i++)
        {
            Vector2 vector = new Vector2(v[3].x, v[3].y);
            v[3].x = v[2].y;
            v[3].y = v[2].x;
            v[2].x = v[1].y;
            v[2].y = v[1].x;
            v[1].x = v[0].y;
            v[1].y = v[0].x;
            v[0].x = vector.y;
            v[0].y = vector.x;
        }
    }

    protected void SetAtlasSprite(UIAtlas.Sprite sp)
    {
        base.mChanged = true;
        this.mSpriteSet = true;
        if (sp != null)
        {
            this.mSprite = sp;
            this.mSpriteName = this.mSprite.name;
        }
        else
        {
            this.mSpriteName = (this.mSprite == null) ? string.Empty : this.mSprite.name;
            this.mSprite = sp;
        }
    }

    protected void SimpleFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
    {
        Vector2 item = new Vector2(this.mOuterUV.xMin, this.mOuterUV.yMin);
        Vector2 vector2 = new Vector2(this.mOuterUV.xMax, this.mOuterUV.yMax);
        verts.Add(new Vector3(1f, 0f, 0f));
        verts.Add(new Vector3(1f, -1f, 0f));
        verts.Add(new Vector3(0f, -1f, 0f));
        verts.Add(new Vector3(0f, 0f, 0f));
        uvs.Add(vector2);
        uvs.Add(new Vector2(vector2.x, item.y));
        uvs.Add(item);
        uvs.Add(new Vector2(item.x, vector2.y));
        Color c = base.color;
        c.a *= base.mPanel.alpha;
        Color32 color2 = !this.atlas.premultipliedAlpha ? c : NGUITools.ApplyPMA(c);
        cols.Add(color2);
        cols.Add(color2);
        cols.Add(color2);
        cols.Add(color2);
    }

    protected void SlicedFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
    {
        if (this.mOuterUV == this.mInnerUV)
        {
            this.SimpleFill(verts, uvs, cols);
        }
        else
        {
            Vector2[] vectorArray = new Vector2[4];
            Vector2[] vectorArray2 = new Vector2[4];
            Texture mainTexture = this.mainTexture;
            vectorArray[0] = Vector2.zero;
            vectorArray[1] = Vector2.zero;
            vectorArray[2] = new Vector2(1f, -1f);
            vectorArray[3] = new Vector2(1f, -1f);
            if (mainTexture == null)
            {
                for (int j = 0; j < 4; j++)
                {
                    vectorArray2[j] = Vector2.zero;
                }
            }
            else
            {
                float pixelSize = this.atlas.pixelSize;
                float num2 = (this.mInnerUV.xMin - this.mOuterUV.xMin) * pixelSize;
                float num3 = (this.mOuterUV.xMax - this.mInnerUV.xMax) * pixelSize;
                float num4 = (this.mInnerUV.yMax - this.mOuterUV.yMax) * pixelSize;
                float num5 = (this.mOuterUV.yMin - this.mInnerUV.yMin) * pixelSize;
                Vector3 localScale = base.cachedTransform.localScale;
                localScale.x = Mathf.Max(0f, localScale.x);
                localScale.y = Mathf.Max(0f, localScale.y);
                Vector2 vector2 = new Vector2(localScale.x / ((float) mainTexture.width), localScale.y / ((float) mainTexture.height));
                Vector2 vector3 = new Vector2(num2 / vector2.x, num4 / vector2.y);
                Vector2 vector4 = new Vector2(num3 / vector2.x, num5 / vector2.y);
                UIWidget.Pivot pivot = base.pivot;
                switch (pivot)
                {
                    case UIWidget.Pivot.Right:
                    case UIWidget.Pivot.TopRight:
                    case UIWidget.Pivot.BottomRight:
                        vectorArray[0].x = Mathf.Min((float) 0f, (float) (1f - (vector4.x + vector3.x)));
                        vectorArray[1].x = vectorArray[0].x + vector3.x;
                        vectorArray[2].x = vectorArray[0].x + Mathf.Max(vector3.x, 1f - vector4.x);
                        vectorArray[3].x = vectorArray[0].x + Mathf.Max((float) (vector3.x + vector4.x), (float) 1f);
                        break;

                    default:
                        vectorArray[1].x = vector3.x;
                        vectorArray[2].x = Mathf.Max(vector3.x, 1f - vector4.x);
                        vectorArray[3].x = Mathf.Max((float) (vector3.x + vector4.x), (float) 1f);
                        break;
                }
                switch (pivot)
                {
                    case UIWidget.Pivot.Bottom:
                    case UIWidget.Pivot.BottomLeft:
                    case UIWidget.Pivot.BottomRight:
                        vectorArray[0].y = Mathf.Max((float) 0f, (float) (-1f - (vector4.y + vector3.y)));
                        vectorArray[1].y = vectorArray[0].y + vector3.y;
                        vectorArray[2].y = vectorArray[0].y + Mathf.Min(vector3.y, -1f - vector4.y);
                        vectorArray[3].y = vectorArray[0].y + Mathf.Min((float) (vector3.y + vector4.y), (float) -1f);
                        break;

                    default:
                        vectorArray[1].y = vector3.y;
                        vectorArray[2].y = Mathf.Min(vector3.y, -1f - vector4.y);
                        vectorArray[3].y = Mathf.Min((float) (vector3.y + vector4.y), (float) -1f);
                        break;
                }
                vectorArray2[0] = new Vector2(this.mOuterUV.xMin, this.mOuterUV.yMax);
                vectorArray2[1] = new Vector2(this.mInnerUV.xMin, this.mInnerUV.yMax);
                vectorArray2[2] = new Vector2(this.mInnerUV.xMax, this.mInnerUV.yMin);
                vectorArray2[3] = new Vector2(this.mOuterUV.xMax, this.mOuterUV.yMin);
            }
            Color c = base.color;
            c.a *= base.mPanel.alpha;
            Color32 item = !this.atlas.premultipliedAlpha ? c : NGUITools.ApplyPMA(c);
            for (int i = 0; i < 3; i++)
            {
                int index = i + 1;
                for (int k = 0; k < 3; k++)
                {
                    if ((this.mFillCenter || (i != 1)) || (k != 1))
                    {
                        int num10 = k + 1;
                        verts.Add(new Vector3(vectorArray[index].x, vectorArray[k].y, 0f));
                        verts.Add(new Vector3(vectorArray[index].x, vectorArray[num10].y, 0f));
                        verts.Add(new Vector3(vectorArray[i].x, vectorArray[num10].y, 0f));
                        verts.Add(new Vector3(vectorArray[i].x, vectorArray[k].y, 0f));
                        uvs.Add(new Vector2(vectorArray2[index].x, vectorArray2[k].y));
                        uvs.Add(new Vector2(vectorArray2[index].x, vectorArray2[num10].y));
                        uvs.Add(new Vector2(vectorArray2[i].x, vectorArray2[num10].y));
                        uvs.Add(new Vector2(vectorArray2[i].x, vectorArray2[k].y));
                        cols.Add(item);
                        cols.Add(item);
                        cols.Add(item);
                        cols.Add(item);
                    }
                }
            }
        }
    }

    protected void TiledFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
    {
        Texture mainTexture = this.material.mainTexture;
        if (mainTexture != null)
        {
            Rect mInner = this.mInner;
            if (this.atlas.coordinates == UIAtlas.Coordinates.TexCoords)
            {
                mInner = NGUIMath.ConvertToPixels(mInner, mainTexture.width, mainTexture.height, true);
            }
            Vector2 localScale = base.cachedTransform.localScale;
            float pixelSize = this.atlas.pixelSize;
            float num2 = Mathf.Abs((float) (mInner.width / localScale.x)) * pixelSize;
            float num3 = Mathf.Abs((float) (mInner.height / localScale.y)) * pixelSize;
            if ((num2 < 0.01f) || (num3 < 0.01f))
            {
                Debug.LogWarning("The tiled sprite (" + NGUITools.GetHierarchy(base.gameObject) + ") is too small.\nConsider using a bigger one.");
                num2 = 0.01f;
                num3 = 0.01f;
            }
            Vector2 vector2 = new Vector2(mInner.xMin / ((float) mainTexture.width), mInner.yMin / ((float) mainTexture.height));
            Vector2 vector3 = new Vector2(mInner.xMax / ((float) mainTexture.width), mInner.yMax / ((float) mainTexture.height));
            Vector2 vector4 = vector3;
            Color c = base.color;
            c.a *= base.mPanel.alpha;
            Color32 item = !this.atlas.premultipliedAlpha ? c : NGUITools.ApplyPMA(c);
            for (float i = 0f; i < 1f; i += num3)
            {
                float x = 0f;
                vector4.x = vector3.x;
                float num6 = i + num3;
                if (num6 > 1f)
                {
                    vector4.y = vector2.y + (((vector3.y - vector2.y) * (1f - i)) / (num6 - i));
                    num6 = 1f;
                }
                while (x < 1f)
                {
                    float num7 = x + num2;
                    if (num7 > 1f)
                    {
                        vector4.x = vector2.x + (((vector3.x - vector2.x) * (1f - x)) / (num7 - x));
                        num7 = 1f;
                    }
                    verts.Add(new Vector3(num7, -i, 0f));
                    verts.Add(new Vector3(num7, -num6, 0f));
                    verts.Add(new Vector3(x, -num6, 0f));
                    verts.Add(new Vector3(x, -i, 0f));
                    uvs.Add(new Vector2(vector4.x, 1f - vector2.y));
                    uvs.Add(new Vector2(vector4.x, 1f - vector4.y));
                    uvs.Add(new Vector2(vector2.x, 1f - vector4.y));
                    uvs.Add(new Vector2(vector2.x, 1f - vector2.y));
                    cols.Add(item);
                    cols.Add(item);
                    cols.Add(item);
                    cols.Add(item);
                    x += num2;
                }
            }
        }
    }

    public override void Update()
    {
        base.Update();
        if (base.mChanged || !this.mSpriteSet)
        {
            this.mSpriteSet = true;
            this.mSprite = null;
            base.mChanged = true;
            this.UpdateUVs(true);
        }
        else
        {
            this.UpdateUVs(false);
        }
    }

    public virtual void UpdateUVs(bool force)
    {
        if (((this.type == Type.Sliced) || (this.type == Type.Tiled)) && (base.cachedTransform.localScale != this.mScale))
        {
            this.mScale = base.cachedTransform.localScale;
            base.mChanged = true;
        }
        if (this.isValid && force)
        {
            Texture mainTexture = this.mainTexture;
            if (mainTexture != null)
            {
                this.mInner = this.mSprite.inner;
                this.mOuter = this.mSprite.outer;
                this.mInnerUV = this.mInner;
                this.mOuterUV = this.mOuter;
                if (this.atlas.coordinates == UIAtlas.Coordinates.Pixels)
                {
                    this.mOuterUV = NGUIMath.ConvertToTexCoords(this.mOuterUV, mainTexture.width, mainTexture.height);
                    this.mInnerUV = NGUIMath.ConvertToTexCoords(this.mInnerUV, mainTexture.width, mainTexture.height);
                }
            }
        }
    }

    public UIAtlas atlas
    {
        get
        {
            return this.mAtlas;
        }
        set
        {
            if (this.mAtlas != value)
            {
                this.mAtlas = value;
                this.mSpriteSet = false;
                this.mSprite = null;
                this.material = (this.mAtlas == null) ? null : this.mAtlas.spriteMaterial;
                if ((string.IsNullOrEmpty(this.mSpriteName) && (this.mAtlas != null)) && (this.mAtlas.spriteList.Count > 0))
                {
                    this.SetAtlasSprite(this.mAtlas.spriteList[0]);
                    this.mSpriteName = this.mSprite.name;
                }
                if (!string.IsNullOrEmpty(this.mSpriteName))
                {
                    string mSpriteName = this.mSpriteName;
                    this.mSpriteName = string.Empty;
                    this.spriteName = mSpriteName;
                    base.mChanged = true;
                    this.UpdateUVs(true);
                }
            }
        }
    }

    public override Vector4 border
    {
        get
        {
            if (this.type != Type.Sliced)
            {
                return base.border;
            }
            UIAtlas.Sprite atlasSprite = this.GetAtlasSprite();
            if (atlasSprite == null)
            {
                return Vector2.zero;
            }
            Rect outer = atlasSprite.outer;
            Rect inner = atlasSprite.inner;
            Texture mainTexture = this.mainTexture;
            if ((this.atlas.coordinates == UIAtlas.Coordinates.TexCoords) && (mainTexture != null))
            {
                outer = NGUIMath.ConvertToPixels(outer, mainTexture.width, mainTexture.height, true);
                inner = NGUIMath.ConvertToPixels(inner, mainTexture.width, mainTexture.height, true);
            }
            return (Vector4) (new Vector4(inner.xMin - outer.xMin, inner.yMin - outer.yMin, outer.xMax - inner.xMax, outer.yMax - inner.yMax) * this.atlas.pixelSize);
        }
    }

    public float fillAmount
    {
        get
        {
            return this.mFillAmount;
        }
        set
        {
            float num = Mathf.Clamp01(value);
            if (this.mFillAmount != num)
            {
                this.mFillAmount = num;
                base.mChanged = true;
            }
        }
    }

    public bool fillCenter
    {
        get
        {
            return this.mFillCenter;
        }
        set
        {
            if (this.mFillCenter != value)
            {
                this.mFillCenter = value;
                this.MarkAsChanged();
            }
        }
    }

    public FillDirection fillDirection
    {
        get
        {
            return this.mFillDirection;
        }
        set
        {
            if (this.mFillDirection != value)
            {
                this.mFillDirection = value;
                base.mChanged = true;
            }
        }
    }

    public Rect innerUV
    {
        get
        {
            this.UpdateUVs(false);
            return this.mInnerUV;
        }
    }

    public bool invert
    {
        get
        {
            return this.mInvert;
        }
        set
        {
            if (this.mInvert != value)
            {
                this.mInvert = value;
                base.mChanged = true;
            }
        }
    }

    public bool isValid
    {
        get
        {
            return (this.GetAtlasSprite() != null);
        }
    }

    public override Material material
    {
        get
        {
            Material material = base.material;
            if (material == null)
            {
                material = (this.mAtlas == null) ? null : this.mAtlas.spriteMaterial;
                this.mSprite = null;
                this.material = material;
                if (material != null)
                {
                    this.UpdateUVs(true);
                }
            }
            return material;
        }
    }

    public Rect outerUV
    {
        get
        {
            this.UpdateUVs(false);
            return this.mOuterUV;
        }
    }

    public override bool pixelPerfectAfterResize
    {
        get
        {
            return (this.type == Type.Sliced);
        }
    }

    public override Vector4 relativePadding
    {
        get
        {
            if (this.isValid && (this.type == Type.Simple))
            {
                return new Vector4(this.mSprite.paddingLeft, this.mSprite.paddingTop, this.mSprite.paddingRight, this.mSprite.paddingBottom);
            }
            return base.relativePadding;
        }
    }

    public string spriteName
    {
        get
        {
            return this.mSpriteName;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                if (!string.IsNullOrEmpty(this.mSpriteName))
                {
                    this.mSpriteName = string.Empty;
                    this.mSprite = null;
                    base.mChanged = true;
                    this.mSpriteSet = false;
                }
            }
            else if (this.mSpriteName != value)
            {
                this.mSpriteName = value;
                this.mSprite = null;
                base.mChanged = true;
                this.mSpriteSet = false;
                if (this.isValid)
                {
                    this.UpdateUVs(true);
                }
            }
        }
    }

    public virtual Type type
    {
        get
        {
            return this.mType;
        }
        set
        {
            if (this.mType != value)
            {
                this.mType = value;
                this.MarkAsChanged();
            }
        }
    }

    public enum FillDirection
    {
        Horizontal,
        Vertical,
        Radial90,
        Radial180,
        Radial360
    }

    public enum Type
    {
        Simple,
        Sliced,
        Tiled,
        Filled
    }
}

