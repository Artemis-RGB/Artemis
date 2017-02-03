using System;
using Betwixt;

namespace Artemis.Profiles.Layers.Models
{
    public class TweenModel
    {
        private readonly LayerModel _layerModel;
        private float _height;
        private Tweener<float> _heightTweener;
        private float _opacity;
        private Tweener<float> _opacityTweener;
        private float _width;
        private Tweener<float> _widthTweener;
        private Tweener<float> _xTweener;
        private Tweener<float> _yTweener;

        public TweenModel(LayerModel layerModel)
        {
            _layerModel = layerModel;
            _width = (float) _layerModel.Width;
            _height = (float) _layerModel.Height;
            _opacity = (float) _layerModel.Opacity;
        }

        public void Update()
        {
            UpdateWidth();
            UpdateHeight();
            UpdateOpacity();
        }

        private void UpdateWidth()
        {
            if (_layerModel.Properties.WidthEaseTime < 0.001)
                return;

            if (_widthTweener == null)
            {
                var widthFunc = GetEaseFunction(_layerModel.Properties.WidthEase);
                var widthSpeed = _layerModel.Properties.WidthEaseTime;

                _xTweener = new Tweener<float>(0, (float) _layerModel.X, widthSpeed, widthFunc, LerpFuncFloat);
                _widthTweener = new Tweener<float>(0, (float) _layerModel.Width, widthSpeed, widthFunc, LerpFuncFloat);
            }

            // Width
            if (Math.Abs(_layerModel.Width - _width) > 0.001)
            {
                _xTweener.Reset((float) _layerModel.X);
                _xTweener.Start();
                _widthTweener.Reset((float) _layerModel.Width);
                _widthTweener.Start();
            }

            _xTweener.Update(40);
            _widthTweener.Update(40);

            _width = (float) _layerModel.Width;

            _layerModel.X = _xTweener.Value;
            _layerModel.Width = _widthTweener.Value;
        }

        private void UpdateHeight()
        {
            if (_layerModel.Properties.HeightEaseTime < 0.001)
                return;

            if (_heightTweener == null)
            {
                var heightFunc = GetEaseFunction(_layerModel.Properties.HeightEase);
                var heightSpeed = _layerModel.Properties.HeightEaseTime;
                _yTweener = new Tweener<float>(0, (float) _layerModel.Y, heightSpeed, heightFunc, LerpFuncFloat);
                _heightTweener = new Tweener<float>(0, (float) _layerModel.Height, heightSpeed, heightFunc,
                    LerpFuncFloat);
            }

            // Height
            if (Math.Abs(_layerModel.Height - _height) > 0.001)
            {
                _yTweener.Reset((float) _layerModel.Y);
                _yTweener.Start();
                _heightTweener.Reset((float) _layerModel.Height);
                _heightTweener.Start();
            }

            _yTweener.Update(40);
            _heightTweener.Update(40);

            _height = (float) _layerModel.Height;

            _layerModel.Y = _yTweener.Value;
            _layerModel.Height = _heightTweener.Value;
        }

        private void UpdateOpacity()
        {
            if (_layerModel.Properties.OpacityEaseTime < 0.001)
                return;

            if (_opacityTweener == null)
            {
                var opacityFunc = GetEaseFunction(_layerModel.Properties.OpacityEase);
                var opacitySpeed = _layerModel.Properties.OpacityEaseTime;
                _opacityTweener = new Tweener<float>(0, (float) _layerModel.Opacity, opacitySpeed, opacityFunc,
                    LerpFuncFloat);
            }

            // Opacity
            if (Math.Abs(_layerModel.Opacity - _opacity) > 0.001)
            {
                _opacityTweener.Reset((float) _layerModel.Opacity);
                _opacityTweener.Start();
            }

            _opacityTweener.Update(40);

            _opacity = (float) _layerModel.Opacity;

            _layerModel.Opacity = _opacityTweener.Value;
        }

        private static EaseFunc GetEaseFunction(string functionName)
        {
            switch (functionName)
            {
                case "In":
                    return Ease.Quint.In;
                case "Out":
                    return Ease.Quint.Out;
                case "In/out":
                    return Ease.Quint.InOut;
                default:
                    return Ease.Linear;
            }
        }

        private static float LerpFuncFloat(float start, float end, float percent)
        {
            return start + (end - start) * percent;
        }
    }
}