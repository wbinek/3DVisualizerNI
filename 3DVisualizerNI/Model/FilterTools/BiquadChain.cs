using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DVisualizerNI.Model.FilterTools
{
    internal class BiquadChain
    {
        private double _xn1, _xn2;
        private double[] _yn, _yn1, _yn2;
        private double[] _yn3, _yn4;
        private int numFilters;

        // Fourth order section variables
        private double xn3, xn4;


        private BiquadChain()
        {
        }


        private BiquadChain(int count)
        {
            allocate(count);
            reset();
        }

        public void allocate(int count)
        {
            numFilters = count;
            Array.Resize(ref _yn, numFilters);
            Array.Resize(ref _yn1, numFilters);
            Array.Resize(ref _yn2, numFilters);

            // Fourth order sections
            Array.Resize(ref _yn3, numFilters);
            Array.Resize(ref _yn4, numFilters);
        }

        private void resize(int count)
        {
            allocate(count);
        }


        private void reset()
        {
            _xn1 = 0;
            _xn2 = 0;

            for (var i = 0; i < numFilters; i++)
            {
                _yn[i] = 0;
                _yn1[i] = 0;
                _yn2[i] = 0;

                // Fourth order sections
                _yn3[i] = 0;
                _yn4[i] = 0;
            }
        }


        private void processBiquad(float input, float output, int stride, int count, Biquad[] coeffs)
        {
            var yn = _yn;
            var yn1 = _yn1;
            var yn2 = _yn2;

            for (var n = 0; n < count; n++)
            {
                double xn = input;

                yn[0] = coeffs[0].b0 * xn + coeffs[0].b1 * _xn1 + coeffs[0].b2 * _xn2
                        + coeffs[0].a1 * yn1[0] + coeffs[0].a2 * yn2[0];

                for (var i = 1; i < numFilters; i++)
                    yn[i] = coeffs[i].b0 * yn[i - 1] + coeffs[i].b1 * yn1[i - 1] + coeffs[i].b2 * yn2[i - 1]
                            + coeffs[i].a1 * yn1[i] + coeffs[i].a2 * yn2[i];

                // Shift delay line elements.
                for (var i = 0; i < numFilters; i++)
                {
                    yn2[i] = yn1[i];
                    yn1[i] = yn[i];
                }
                _xn2 = _xn1;
                _xn1 = xn;

                // Store result and stride
                output = (float)yn[numFilters - 1];

                input += stride;
                output += stride;
            }
        }


        private void processFourthOrderSections(float input, float output, int stride, int count, Biquad[] coeffs)
        {
            var yn = _yn;
            var yn1 = _yn1;
            var yn2 = _yn2;
            var yn3 = _yn3;
            var yn4 = _yn4;

            for (var n = 0; n < count; n++)
            {
                double xn = input;

                yn[0] = coeffs[0].b0 * xn
                        + coeffs[0].b1 * _xn1
                        + coeffs[0].b2 * _xn2
                        + coeffs[0].b3 * xn3
                        + coeffs[0].b4 * xn4 +
                        coeffs[0].a1 * yn1[0]
                        + coeffs[0].a2 * yn2[0]
                        + coeffs[0].a3 * yn3[0]
                        + coeffs[0].a4 * yn4[0];

                for (var i = 1; i < numFilters; i++)
                    yn[i] = coeffs[i].b0 * yn[i - 1]
                            + coeffs[i].b1 * yn1[i - 1]
                            + coeffs[i].b2 * yn2[i - 1]
                            + coeffs[i].b3 * yn3[i - 1]
                            + coeffs[i].b4 * yn4[i - 1] +
                            coeffs[i].a1 * yn1[i]
                            + coeffs[i].a2 * yn2[i]
                            + coeffs[i].a3 * yn3[i]
                            + coeffs[i].a4 * yn4[i];

                // Shift delay line elements.
                for (var i = 0; i < numFilters; i++)
                {
                    yn4[i] = yn3[i];
                    yn3[i] = yn2[i];
                    yn2[i] = yn1[i];
                    yn1[i] = yn[i];
                }

                xn4 = xn3;
                xn3 = _xn2;
                _xn2 = _xn1;
                _xn1 = xn;

                // Store result and stride
                output = (float)yn[numFilters - 1];

                input += stride;
                output += stride;
            }
        }
    }
}
