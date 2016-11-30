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
        private float _x;
        private Tweener<float> _xTweener;
        private float _y;
        private Tweener<float> _yTweener;

        public TweenModel(LayerModel layerModel, double defaultDuration)
        {
            _layerModel = layerModel;

            XDuration = defaultDuration;
            YDuration = defaultDuration;
            WidthDuration = defaultDuration;
            HeightDuration = defaultDuration;
            OpacityDuration = defaultDuration;

            XFunc = Ease.Quad.InOut;
            YFunc  = Ease.Quad.InOut;
            WidthFunc = Ease.Quad.InOut;
            HeightFunc = Ease.Quad.InOut;
            OpacityFunc = Ease.Quad.InOut;

            _xTweener = new Tweener<float>(0, (float) layerModel.X, XDuration, XFunc);
            _yTweener = new Tweener<float>(0, (float) layerModel.Y, YDuration, YFunc);
            _widthTweener = new Tweener<float>(0, (float) layerModel.Width, WidthDuration, WidthFunc);
            _heightTweener = new Tweener<float>(0, (float) layerModel.Height, HeightDuration, HeightFunc);
            _opacityTweener = new Tweener<float>(0, (float) layerModel.Opacity, OpacityDuration, OpacityFunc);

            StoreCurrentValues();
        }

        public double XDuration { get; set; }
        public double YDuration { get; set; }
        public double WidthDuration { get; set; }
        public double HeightDuration { get; set; }
        public double OpacityDuration { get; set; }

        public EaseFunc XFunc { get; set; }
        public EaseFunc YFunc { get; set; }
        public EaseFunc WidthFunc { get; set; }
        public EaseFunc HeightFunc { get; set; }
        public EaseFunc OpacityFunc { get; set; }

        private void StoreCurrentValues()
        {
            _x = (float) _layerModel.X;
            _y = (float) _layerModel.Y;
            _width = (float) _layerModel.Width;
            _height = (float) _layerModel.Height;
            _opacity = (float) _layerModel.Opacity;
        }

        public void Update()
        {
            if (Math.Abs(_layerModel.X - _x) > 0.001)
                _xTweener = new Tweener<float>(_x, (float) _layerModel.X, XDuration, XFunc);
            if (Math.Abs(_layerModel.Y - _y) > 0.001)
                _yTweener = new Tweener<float>(_y, (float) _layerModel.Y, YDuration, YFunc);
            if (Math.Abs(_layerModel.Width - _width) > 0.001)
                _widthTweener = new Tweener<float>(_width, (float) _layerModel.Width, WidthDuration, WidthFunc);
            if (Math.Abs(_layerModel.Height - _height) > 0.001)
                _heightTweener = new Tweener<float>(_height, (float) _layerModel.Height, HeightDuration, HeightFunc);
            if (Math.Abs(_layerModel.Opacity - _opacity) > 0.001)
                _opacityTweener = new Tweener<float>(_opacity, (float) _layerModel.Opacity, OpacityDuration, OpacityFunc);

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
    }
}