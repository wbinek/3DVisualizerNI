using System;

namespace _3DVisualizerNI.Helpers.ButterworthFilterDesign
{
    class BiquadChain
    {
		int numFilters;
        double _xn1, _xn2;
        double[] _yn, _yn1, _yn2;

        // Fourth order section variables
        double xn3, xn4;
        double[] _yn3, _yn4;


        BiquadChain()
        {
        }


        BiquadChain(int count)
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

        void resize(int count)
        {
            allocate(count);
        }


        void reset()
        {
            _xn1 = 0;
            _xn2 = 0;

            for (int i = 0; i < numFilters; i++)
            {
                _yn[i] = 0;
                _yn1[i] = 0;
                _yn2[i] = 0;

                // Fourth order sections
                _yn3[i] = 0;
                _yn4[i] = 0;
            }
        }


        void processBiquad(float input, float output, int stride, int count, Biquad[] coeffs)
        {

            double[] yn = _yn;
            double[] yn1 = _yn1;
            double[] yn2 = _yn2;

            for (int n = 0; n < count; n++)
            {
                double xn = input;

                yn[0] = coeffs[0].b0 * xn + coeffs[0].b1 * _xn1 + coeffs[0].b2 * _xn2
                        + coeffs[0].a1 * yn1[0] + coeffs[0].a2 * yn2[0];

                for (int i = 1; i < numFilters; i++)
                {
                    yn[i] = coeffs[i].b0 * yn[i - 1] + coeffs[i].b1 * yn1[i - 1] + coeffs[i].b2 * yn2[i - 1]
                            + coeffs[i].a1 * yn1[i] + coeffs[i].a2 * yn2[i];
                }

                // Shift delay line elements.
                for (int i = 0; i < numFilters; i++)
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


        void processFourthOrderSections(float input, float output, int stride, int count, Biquad[] coeffs)
        {

            double[] yn = _yn;
            double[] yn1 = _yn1;
            double[] yn2 = _yn2;
            double[] yn3 = _yn3;
            double[] yn4 = _yn4;

            for (int n = 0; n < count; n++)
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

                for (int i = 1; i < numFilters; i++)
                {
                    yn[i] = coeffs[i].b0 * yn[i - 1]
                            + coeffs[i].b1 * yn1[i - 1]
                            + coeffs[i].b2 * yn2[i - 1]
                            + coeffs[i].b3 * yn3[i - 1]
                            + coeffs[i].b4 * yn4[i - 1] +

                            coeffs[i].a1 * yn1[i]
                            + coeffs[i].a2 * yn2[i]
                            + coeffs[i].a3 * yn3[i]
                            + coeffs[i].a4 * yn4[i];
                }

                // Shift delay line elements.
                for (int i = 0; i < numFilters; i++)
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
