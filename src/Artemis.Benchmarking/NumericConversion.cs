using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core;
using BenchmarkDotNet.Attributes;

namespace Artemis.Benchmarking
{
    public class NumericConversion
    {
        private readonly float _float;
        private readonly Numeric _numeric;

        public NumericConversion()
        {
            _float = 255235235f;
            _numeric = new Numeric(_float);
        }

        [Benchmark]
        public void FloatToIntCast()
        {
            var integer = (int) _float;
        }

        [Benchmark]
        public void NumericToIntCast()
        {
            var integer = (int) _numeric;
        }

        [Benchmark]
        public void NumericToIntConvertTo()
        {
            var integer = Convert.ChangeType(_numeric, typeof(int));
        }
    }
}