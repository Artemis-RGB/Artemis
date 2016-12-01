using System;
using Betwixt;

namespace Artemis.Profiles.Layers.Models
{
    public class TweenModel
    {
        private readonly LayerModel _layerModel;
        private float _x;
        private Tweener<float> _xTweener;
        private float _y;
        private Tweener<float> _yTweener;
        private float _width;
        private Tweener<float> _widthTweener;
        private float _height;
        private Tweener<float> _heightTweener;
        private float _opacity;
        private Tweener<float> _opacityTweener;

        public TweenModel(LayerModel layerModel)
        {
            _layerModel = layerModel;

            _xTweener = new Tweener<float>((float) layerModel.X, (float) layerModel.X, 0);
            _yTweener = new Tweener<float>((float) layerModel.Y, (float) layerModel.Y, 0);
            _widthTweener = new Tweener<float>((float) layerModel.Width, (float) layerModel.Width, 0);
            _heightTweener = new Tweener<float>((float) layerModel.Height, (float) layerModel.Height, 0);
            _opacityTweener = new Tweener<float>((float) layerModel.Opacity, (float) layerModel.Opacity, 0);

            StoreCurrentValues();
        }

        public void Update()
        {
            // TODO: Don't update if animation speed is 0
            if (Math.Abs(_layerModel.X - _x) > 0.001)
                _xTweener = new Tweener<float>(_x, (float) _layerModel.X, _layerModel.Properties.XEaseTime,
                    GetEaseFunction(_layerModel.Properties.XEase));
            if (Math.Abs(_layerModel.Y - _y) > 0.001)
                _yTweener = new Tweener<float>(_y, (float) _layerModel.Y, _layerModel.Properties.YEaseTime,
                    GetEaseFunction(_layerModel.Properties.YEase));
            if (Math.Abs(_layerModel.Width - _width) > 0.001)
                _widthTweener = new Tweener<float>(_width, (float) _layerModel.Width,
                    _layerModel.Properties.WidthEaseTime, GetEaseFunction(_layerModel.Properties.WidthEase));
            if (Math.Abs(_layerModel.Height - _height) > 0.001)
                _heightTweener = new Tweener<float>(_height, (float) _layerModel.Height,
                    _layerModel.Properties.HeightEaseTime, GetEaseFunction(_layerModel.Properties.HeightEase));
            if (Math.Abs(_layerModel.Opacity - _opacity) > 0.001)
                _opacityTweener = new Tweener<float>(_opacity, (float) _layerModel.Opacity,
                    _layerModel.Properties.OpacityEaseTime, GetEaseFunction(_layerModel.Properties.OpacityEase));

            _xTweener.Update(40);
            _yTweener.Update(40);
            _widthTweener.Update(40);
            _heightTweener.Update(40);
            _opacityTweener.Update(40);

            StoreCurrentValues();

            _layerModel.X = _xTweener.Value;
            _layerModel.Y = _yTweener.Value;
            _layerModel.Width = _widthTweener.Value;
            _layerModel.Height = _heightTweener.Value;
            _layerModel.Opacity = _opacityTweener.Value;
        }

        private void StoreCurrentValues()
        {
            _x = (float) _layerModel.X;
            _y = (float) _layerModel.Y;
            _width = (float) _layerModel.Width;
            _height = (float) _layerModel.Height;
            _opacity = (float) _layerModel.Opacity;
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
    }
}