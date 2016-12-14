using System;
using System.Numerics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace _3DVisualizerNI.Helpers.ButterworthFilterDesign
{
    public enum FILTER_TYPE
    {
        kLoPass = 10000,
        kHiPass = 10001,
        kBandPass = 10002,
        kBandStop = 10003,
        kLoShelf = 10004,
        kHiShelf = 10005, // high order EQ
        kParametric = 10006  // high order EQ
    };

    public enum CenterFreqTO
    {
        [Description("25")]
        f25,
        [Description("31.5")]
        f31,
        [Description("40")]
        f40,
        [Description("50")]
        f50,
        [Description("63")]
        f63,
        [Description("80")]
        f80,
        [Description("100")]
        f100,
        [Description("125")]
        f125,
        [Description("160")]
        f160,
        [Description("200")]
        f200,
        [Description("250")]
        f250,
        [Description("315")]
        f315,
        [Description("400")]
        f400,
        [Description("500")]
        f500,
        [Description("630")]
        f630,
        [Description("800")]
        f800,
        [Description("1000")]
        f1000,
        [Description("1250")]
        f1250,
        [Description("1600")]
        f1600,
        [Description("2000")]
        f2000,
        [Description("2500")]
        f2500,
        [Description("3150")]
        f3150,
        [Description("4000")]
        f4000,
        [Description("5000")]
        f5000,
        [Description("6300")]
        f6300,
        [Description("8000")]
        f8000,
        [Description("10000")]
        f10000,
        [Description("12500")]
        f12500,
        [Description("16000")]
        f16000,
    };

    public enum CenterFreqO
    {
        [Description("31")]
        f31=1,
        [Description("63")]
        f63=4,
        [Description("125")]
        f125=7,
        [Description("250")]
        f250=10,
        [Description("500")]
        f500=13,
        [Description("1000")]
        f1000=16,
        [Description("2000")]
        f2000=19,
        [Description("4000")]
        f4000=22,
        [Description("8000")]
        f8000=25,
        [Description("16000")]
        f16000=28,
    };

    public enum FilterBank
    {
        [Description("None")]
        None,
        [Description("Octave")]
        Octave,
        [Description("Third octave")]
        Third_octave,
    };


    public static class Butterworth
    {
        // Internal state used during computation of coefficients
        [ThreadStatic] static double f1, f2;

        [ThreadStatic] static int numPoles, numZeros;
        [ThreadStatic] static Complex[] zeros;
        [ThreadStatic] static Complex[] poles;

        [ThreadStatic] static double Wc; // Omega cutoff == passband edge freq
        [ThreadStatic] static double bw; // Bandwidth

        [ThreadStatic] static double gain;
        [ThreadStatic] static double preBLTgain;

        [ThreadStatic] static int nba;
        [ThreadStatic] static double[] ba;

        public static double[] filterResult(FilterBank bank, Enum freq, double[] signal, int Fs)
        {
            Biquad[] filter = new Biquad[0];
            double gain = 1;
            bandPassBank(Fs, bank, freq, 8, ref filter, ref gain);

            double[][] x = new double[filter.Count()][];
            double[][] y = new double[filter.Count()][];
            for (int i = 0; i < filter.Count(); i++)
            {
                x[i] = new double[5];
                y[i] = new double[4];
            }

            double[] filtered = new double[signal.Length];

            double output = 0;
            for (int index = 0; index < signal.Length; index++)
            {
                int section_count = 0;
                double x0 = signal[index];

                foreach (Biquad section in filter)
                {
                    Array.Copy(x[section_count], 0, x[section_count], 1, x[section_count].Length - 1);
                    x[section_count][0] = x0;

                    //output = section.b0 * x[section_count][0] + section.b1 * x[section_count][1] + section.b2 * x[section_count][2] + section.b3 * x[section_count][3] + section.b4 * x[section_count][4] +
                    //    (section.a1 * y[section_count][0] + section.a2 * y[section_count][1] + section.a3 * y[section_count][2] + section.a4 * y[section_count][3]);

                    output = section.b0 * x[section_count][0] + section.b1 * x[section_count][1] + section.b2 * x[section_count][2] +
                        (section.a1 * y[section_count][0] + section.a2 * y[section_count][1]);

                    Array.Copy(y[section_count], 0, y[section_count], 1, y[section_count].Length - 1);
                    y[section_count][0] = output;
                    x0 = output;
                    section_count++;
                }

                filtered[index] = output * gain;
            }
            return filtered;
        }

        public static bool bandPassBank(double fs, FilterBank bank, Enum centerFreq, int filterOrder,
            ref Biquad[] coeffs, ref double overallGain)
        {
            double freq = getValidFreq(bank, centerFreq);
            double omega=1;
            switch (bank)
            {
                case (FilterBank.Octave):
                    omega = Math.Sqrt(2);
                    break;
                case (FilterBank.Third_octave):
                    omega = 1.02676;
                    //filterOrder += -4;
                    break;
                case (FilterBank.None):
                    return false;                  
            }
            return coefficients(FILTER_TYPE.kBandPass, fs, freq/omega, freq*omega, filterOrder, ref coeffs, ref overallGain);

        }

        public static bool loPass(double fs, double f1, double f2, int filterOrder,
            ref Biquad[] coeffs, double overallGain)
        {
            return coefficients(FILTER_TYPE.kLoPass, fs, f1, f2, filterOrder, ref coeffs, ref overallGain);
        }

        public static bool hiPass(double fs, double f1, double f2, int filterOrder,
                    ref Biquad[] coeffs, double overallGain)
        {
            return coefficients(FILTER_TYPE.kHiPass, fs, f1, f2, filterOrder, ref coeffs, ref overallGain);
        }


        public static bool bandPass(double fs, double f1, double f2, int filterOrder,
                      ref Biquad[] coeffs, double overallGain)
        {
            return coefficients(FILTER_TYPE.kBandPass, fs, f1, f2, filterOrder, ref coeffs, ref overallGain);
        }

        public static bool bandStop(double fs, double f1, double f2, int filterOrder,
                      ref Biquad[] coeffs, double overallGain)
        {
            return coefficients(FILTER_TYPE.kBandStop, fs, f1, f2, filterOrder, ref coeffs, ref overallGain);
        }

        public static double getValidFreq(FilterBank type, Enum freq)
        {
            switch (type)
            {
                case (FilterBank.Octave):
                    switch ((CenterFreqO)freq)
                    {
                        case (CenterFreqO.f31): return 31.25;
                        case (CenterFreqO.f63): return 62.5;
                        case (CenterFreqO.f125): return 125;
                        case (CenterFreqO.f250): return 250;
                        case (CenterFreqO.f500): return 500;
                        case (CenterFreqO.f1000): return 1000;
                        case (CenterFreqO.f2000): return 2000;
                        case (CenterFreqO.f4000): return 4000;
                        case (CenterFreqO.f8000): return 8000;
                        case (CenterFreqO.f16000): return 16000;
                    }
                    break;

                case (FilterBank.Third_octave):
                    switch ((CenterFreqTO)freq)
                    {
                        case (CenterFreqTO.f25): return 24.803;
                        case (CenterFreqTO.f31): return 31.25;
                        case (CenterFreqTO.f40): return 39.373;
                        case (CenterFreqTO.f50): return 49.606;
                        case (CenterFreqTO.f63): return 62.5;
                        case (CenterFreqTO.f80): return 78.745;
                        case (CenterFreqTO.f100): return 99.213;
                        case (CenterFreqTO.f125): return 125;
                        case (CenterFreqTO.f160): return 157.49;
                        case (CenterFreqTO.f200): return 198.43;
                        case (CenterFreqTO.f250): return 250;
                        case (CenterFreqTO.f315): return 314.98;
                        case (CenterFreqTO.f400): return 396.85;
                        case (CenterFreqTO.f500): return 500;
                        case (CenterFreqTO.f630): return 629.96;
                        case (CenterFreqTO.f800): return 793.7;
                        case (CenterFreqTO.f1000): return 1000;
                        case (CenterFreqTO.f1250): return 1259.9;
                        case (CenterFreqTO.f1600): return 1587.4;
                        case (CenterFreqTO.f2000): return 2000;
                        case (CenterFreqTO.f2500): return 2519.8;
                        case (CenterFreqTO.f3150): return 3174.8;
                        case (CenterFreqTO.f4000): return 4000;
                        case (CenterFreqTO.f5000): return 5039.7;
                        case (CenterFreqTO.f6300): return 6349.6;
                        case (CenterFreqTO.f8000): return 8000;
                        case (CenterFreqTO.f10000): return 10079;
                        case (CenterFreqTO.f12500): return 12699;
                        case (CenterFreqTO.f16000): return 16000;
                    }
                    break;
            }
            return 0;
        }

        //******************************************************************************
        // Lowpass analogue prototype. Places Butterworth poles evenly around
        // the S-plane unit circle.
        //
        // Reference: MATLAB buttap(filterOrder)
        //******************************************************************************

        public static List<Complex> prototypeAnalogLowPass(int filterOrder)
        {

            List<Complex> poles = new List<Complex>();

            for (int k = 0; k < (filterOrder + 1) / 2; k++)
            {
                double theta = (double)(2 * k + 1) * Math.PI / (2 * filterOrder);
                double real = -Math.Sin(theta);
                double imag = Math.Cos(theta);
                poles.Add(new Complex(real, imag));
                poles.Add(new Complex(real, -imag)); // conjugate
            }

            return poles;
        }


        //******************************************************************************
        // Generate Butterworth coefficients, the main public method
        //
        // Reference: MATLAB butter(n, Wn, ...)
        //            http://en.wikipedia.org/wiki/Butterworth_filter
        //******************************************************************************

        static bool coefficients(FILTER_TYPE filter, double fs, double freq1_cutoff,

                               double freq2_cutoff, int filterOrder,
                               ref Biquad[] coeffs,
							   ref double overallGain){

            //******************************************************************************
            // Init internal state based on filter design requirements

            Array.Resize(ref zeros, 2 * filterOrder);
            Array.Resize(ref poles, 2 * filterOrder);

	        f1 = freq1_cutoff;
	        f2 = freq2_cutoff;

	        Wc = 0;  // Omega cutoff = passband edge freq
	        bw = 0;


	        //******************************************************************************
	        // Prewarp

	        f1 = 2 * Math.Tan(Math.PI* f1 / fs);
            f2 = 2 * Math.Tan(Math.PI * f2 / fs);


            //******************************************************************************
            // Design basic S-plane poles-only analogue LP prototype

            // Get zeros & poles of prototype analogue low pass.
            List<Complex> tempPoles = prototypeAnalogLowPass(filterOrder);

            // Copy tmppole into poles
            int index = 0;
            foreach(Complex pole in tempPoles)
            {
                poles[index] = pole;
                index++;
            }


            numPoles = (int)tempPoles.Count();
	        numZeros = 0;       // butterworth LP prototype has no zeros.
	        gain = 1.0;         // always 1 for the butterworth prototype lowpass.


	        //******************************************************************************
	        // Convert prototype to target filter type (LP/HP/BP/BS) - S-plane

	        // Re-orient BP/BS corner frequencies if necessary
	        if(f1 > f2){
		        double temp = f2;
            f2 = f1;
		        f1 = temp;
	        }

	        // Cutoff Wc = f2
	        switch(filter){

	         case FILTER_TYPE.kLoPass:

                 convert2lopass();
		         break;

	         case FILTER_TYPE.kHiPass:

                 convert2hipass();
		         break;

	         case FILTER_TYPE.kBandPass:

                 convert2bandpass();
		         break;

	         case FILTER_TYPE.kBandStop:

                 convert2bandstop();
		         break;

	         default:
		         return false;
	        }


	        //******************************************************************************
	        // SANITY CHECK: Ensure poles are in the left half of the S-plane
	        for(int i = 0; i<numPoles; i++){
		        if(poles[i].Real > 0){
			        #if LOG_OUTPUT
				        cerr << "[Butterworth Filter Design] Error: poles must be in the left half plane" << endl;
			        #endif
			        return false;
		        }
	        }


	        //******************************************************************************
	        // Map zeros & poles from S-plane to Z-plane

	        nba = 0;
	        ba = new double[2 * new[] { numPoles, numZeros }.Max() + 5];
	        preBLTgain = gain;

	        if(!s2Z()){
		        #if LOG_OUTPUT
			        cerr << "[Butterworth Filter Design] Error: s2Z failed" << endl << endl;
		        #endif
		        return false;
	        }


	        //******************************************************************************
	        // Split up Z-plane poles and zeros into SOS

	        if(!zp2SOS()){
		        #if LOG_OUTPUT
			        cerr << "[Butterworth Filter Design] Error: zp2SOS failed" << endl << endl;
		        #endif
		        return false;
	        }

	        // correct the overall gain
	        if(filter == FILTER_TYPE.kLoPass || filter == FILTER_TYPE.kBandPass){ // pre-blt is okay for S-plane
		        ba[0] = preBLTgain* (preBLTgain / gain); // 2nd term is how much BLT boosts,
	        }
	        else if(filter == FILTER_TYPE.kHiPass || filter == FILTER_TYPE.kBandStop){ // HF gain != DC gain
		        ba[0] = 1 / ba[0];
	        }


	        //******************************************************************************
	        // Init biquad chain with coefficients from SOS

	        overallGain = ba[0];
	        int numFilters = filterOrder / 2;
	        if(filter == FILTER_TYPE.kBandPass || filter == FILTER_TYPE.kBandStop){
		        numFilters = filterOrder; // we have double the # of biquad sections

		        // IOHAVOC filterOrder is never used again? figure this out FIXME
		        // filterOrder *= 2;
	        }

            Array.Resize(ref coeffs, numFilters);
	        for(int i = 0; i<numFilters; i++){
                coeffs[i] = new Biquad();
		        coeffs[i].DF2TBiquad(1.0,                     // b0
                                       ba[4 * i + 1],           // b1
                                       ba[4 * i + 2],           // b2
							           1.0,                     // a0
                                       ba[4 * i + 3],           // a1
                                       ba[4 * i + 4]);          // a2
	        }

	        #if LOG_OUTPUT

		        // ba[0] contains your gain factor
		        cout << endl;
		        cout << "[Butterworth Filter Design] preBLTgain = " << preBLTgain << endl;
		        cout << "[Butterworth Filter Design] gain = " << gain << endl;

		        cout << "[Butterworth Filter Design] ba[0] = " << ba[0] << endl;
		        cout << "[Butterworth Filter Design] coeff size = " << nba << endl << endl;

		        for(int i = 0; i < (nba - 1) / 4; i++){
			        // b0,b1,b2: a0,a1,a2:= 1.0, ba[4*i+1], ba[4*i+2], 1.0, ba[4*i+3], ba[4*i+4]
			        cout << "[Butterworth Filter Design] Biquads:= 1.0 " << ba[4 * i + 1] << " "
				         << ba[4 * i + 2] << " "
				         << ba[4 * i + 3] << " "
				         << ba[4 * i + 4] << endl;
		        }
	        #endif

	        return true;
        }


        //******************************************************************************
        // High-Order Equalizer Filters: Low-Shelf, High-Shelf & Parametric Boost-Cut
        //
        // Reference: Sophocles J. Orfanidis, "High-Order Digital Parametric Equalizer
        //            Design," J. Audio Eng. Soc., vol.53, pp. 1026-1046, November 2005.
        //            http://eceweb1.rutgers.edu/~orfanidi/hpeq/
        ////******************************************************************************

        public static bool coefficientsEQ(FILTER_TYPE filter, double fs, double f1,
                                         double f2, int filterOrder,
                                         ref  Biquad[] coeffs,
                                         double overallGain)
        {

            // Convert band edges to radians/second
            double w1 = 2.0 * Math.PI * (f1 / fs);
            double w2 = 2.0 * Math.PI * (f2 / fs);

            // Compute center frequency w0 & bandwidth Dw
            // for parametric case in radians/sample
            double Dw = w2 - w1;
            double w0 = Math.Acos(Math.Sin(w1 + w2) / (Math.Sin(w1) + Math.Sin(w2)));
            if (w2 == Math.PI)
            {
                w0 = Math.PI;
            }

            // Setup gain
            double G0 = 1.0;        // Reference gain == 0 dB
            double G = overallGain;
            double GB = 0.75 * G;   // Setup Peak-to-Bandwidth Gain Ratio. What about the 3-dB point??

            G = Math.Pow(10, (G / 20.0));     // G  = 10^(G/20);
            GB = Math.Pow(10, (GB / 20.0));    // GB = 10^(GB/20);

            //  Do not proceed with design if G == G0
            if (G == G0)
            {
                return true;
            }

            int L = filterOrder / 2;
            double c0 = Math.Cos(w0);

            if (w0 == 0)
            {
                c0 = 1.0;
            }
            if (w0 == Math.PI / 2)
            {
                c0 = 0.0;
            }
            if (w0 == Math.PI)
            {
                c0 = -1.0;
            }

            double WB = Math.Tan(Dw / 2.0);
            double epsilon = Math.Sqrt(((G * G) - (GB * GB)) / ((GB * GB) - (G0 * G0)));

            double g = Math.Pow(G, (1.0 / filterOrder));
            double g0 = Math.Pow(G0, (1.0 / filterOrder));

            // Butterworth
            double b = WB / Math.Pow(epsilon, (1.0 / filterOrder));

            // Ensure size 'L' of coeff vector is correct!
            Array.Resize<Biquad>(ref coeffs, L);
            for (int i = 1; i <= L; i++)
            {
                coeffs[i - 1] = new Biquad();
                double phi = (2 * i - 1.0) * Math.PI / (2.0 * filterOrder);
                double si = Math.Sin(phi);
                double D = b * b + 2.0 * b * si + 1.0;

                if (filter == FILTER_TYPE.kLoShelf || filter == FILTER_TYPE.kHiShelf)
                { // Compute SOS coefficients

                    double b0h = (g * g * b * b + 2 * g0 * g * b * si + g0 * g0) / D;
                    double b1h = (filter == FILTER_TYPE.kHiShelf) ? -2.0 * (g * g * b * b - g0 * g0) / D : 2.0 * (g * g * b * b - g0 * g0) / D;
                    double b2h = (g * g * b * b - 2 * g0 * g * b * si + g0 * g0) / D;
                    double a1h = (filter == FILTER_TYPE.kHiShelf) ? -2.0 * (b * b - 1.0) / D : 2.0 * (b * b - 1.0) / D;
                    double a2h = (b * b - 2 * b * si + 1.0) / D;

                    // High-order HP/LP shelving filter coefficients can be expressed as 2nd-order sections (SOS)
                    // i.e. biquads
                    coeffs[i - 1].DF2TBiquad(b0h,         // b0
                                               b1h,         // b1
                                               b2h,         // b2
                                               1.0,         // a0
                                               a1h,         // a1
                                               a2h);        // a2
                }
                else if (filter == FILTER_TYPE.kParametric)
                { // Compute 4th order sections

                    double b0 = (g * g * b * b + g0 * g0 + 2 * g * g0 * si * b) / D;
                    double b1 = -4 * c0 * (g0 * g0 + g * g0 * si * b) / D;
                    double b2 = 2 * ((1 + 2 * c0 * c0) * g0 * g0 - g * g * b * b) / D;
                    double b3 = -4 * c0 * (g0 * g0 - g * g0 * si * b) / D;
                    double b4 = (g * g * b * b + g0 * g0 - 2 * g * g0 * si * b) / D;
                    double a1 = -4 * c0 * (1 + si * b) / D;
                    double a2 = 2 * (1 + 2 * c0 * c0 - b * b) / D;
                    double a3 = -4 * c0 * (1 - si * b) / D;
                    double a4 = (b * b - 2 * si * b + 1) / D;

                    // Parameteric EQ filter coefficients (like band pass & band stop) are twice the
                    // specified filter order. This is normal and by design. Unlike bandpass & bandstop
                    // though, the realization via the Bilinear Transform (BLT) renders 4th order sections.
                    // So rather than split 4th order sections into 2nd order sections (biquads),
                    // with fancy polynomial root factoring, we use them as is.
                    // There are no stability issues for sections this size.
                    coeffs[i - 1].DF2TFourthOrderSection(b0,          // b0
                                                           b1,          // b1
                                                           b2,          // b2
                                                           b3,          // b3
                                                           b4,          // b4
                                                           1.0,         // a0
                                                           a1,          // a1
                                                           a2,          // a2
                                                           a3,          // a3
                                                           a4);         // a4
                }
            }

            return true;
        }



        //******************************************************************************
        //
        // Z = (2 + S) / (2 - S) is the S-plane to Z-plane bilinear transform
        //
        // Reference: http://en.wikipedia.org/wiki/Bilinear_transform
        //
        //******************************************************************************

        static double blt(ref Complex sz)
        {

            Complex two = new Complex(2.0, 0);
            Complex s = sz;      // sz is an input(S-plane) & output(Z-plane) arg
            sz = (two + s) / (two - s);

            // return the gain
            return (two - s).Magnitude;
        }


        //******************************************************************************
        //
        // Convert poles & zeros from S-plane to Z-plane via Bilinear Tranform (BLT)
        //
        //******************************************************************************

        static bool s2Z()
        {

            // blt zeros
            for (int i = 0; i < numZeros; i++)
            {
                gain /= blt(ref zeros[i]);
            }

            // blt poles
            for (int i = 0; i < numPoles; i++)
            {
                gain *= blt(ref poles[i]);
            }

            return true;
        }


        //******************************************************************************
        //
        // Convert filter poles and zeros to second-order sections
        //
        // Reference: http://www.mathworks.com/help/signal/ref/zp2sos.html
        //
        //******************************************************************************

        static bool zp2SOS()
        {

            int filterOrder = new[] { numPoles, numZeros }.Max();
            Complex[] zerosTempVec = new Complex[filterOrder];
            Complex[] polesTempVec = new Complex[filterOrder];

            // Copy
            for (int i = 0; i < numZeros; i++)
            {
                zerosTempVec[i] = zeros[i];
            }

            // Add zeros at -1, so if S-plane degenerate case where
            // numZeros = 0 will map to -1 in Z-plane.
            for (int i = numZeros; i < filterOrder; i++)
            {
                zerosTempVec[i] = new Complex(-1, 0);
            }

            // Copy
            for (int i = 0; i < numPoles; i++)
            {
                polesTempVec[i] = poles[i];
            }

            ba[0] = gain; // store gain

            int numSOS = 0;
            for (int i = 0; i + 1 < filterOrder; i += 2, numSOS++)
            {
                ba[4 * numSOS + 1] = -(zerosTempVec[i] + zerosTempVec[i + 1]).Real;
                ba[4 * numSOS + 2] = (zerosTempVec[i] * zerosTempVec[i + 1]).Real;
                ba[4 * numSOS + 3] = -(polesTempVec[i] + polesTempVec[i + 1]).Real;
                ba[4 * numSOS + 4] = (polesTempVec[i] * polesTempVec[i + 1]).Real;
            }

            // Odd filter order thus one pair of poles/zeros remains
            if (filterOrder % 2 == 1)
            {
                ba[4 * numSOS + 1] = -zerosTempVec[filterOrder - 1].Real;
                ba[4 * numSOS + 2] = ba[4 * numSOS + 4] = 0;
                ba[4 * numSOS + 3] = -polesTempVec[filterOrder - 1].Real;
                numSOS++;
            }

            // Set output param
            nba = 1 + 4 * numSOS;
            return true;
        }




        //******************************************************************************
        // Convert analog lowpass prototype poles to lowpass
        //******************************************************************************

        static void convert2lopass()
        {
            Wc = f2;                                   // critical frequency

            gain *= Math.Pow(Wc, numPoles);

            numZeros = 0;                              // poles only
            for (int i = 0; i < numPoles; i++)
            {    // scale poles by the cutoff Wc
                poles[i] = Wc * poles[i];
            }
        }


        //******************************************************************************
        // Convert lowpass poles & zeros to highpass
        // with Wc = f2, use:  hp_S = Wc / lp_S;
        //******************************************************************************

        static void convert2hipass()
        {
            Wc = f2;   // Critical frequency

            // Calculate gain
            Complex prodz = new Complex(1.0, 0.0);
            Complex prodp = new Complex(1.0, 0.0);

            for (int i = 0; i < numZeros; i++)
            {
                prodz *= -zeros[i];
            }

            for (int i = 0; i < numPoles; i++)
            {
                prodp *= -poles[i];
            }

            gain *= prodz.Real / prodp.Real;

            // Convert LP poles to HP
            for (int i = 0; i < numPoles; i++)
            {
                if (poles[i].Magnitude!=0)             //Nie wiem czy to jest dobrze. było if(abs(poles[i]))
                {
                    poles[i] = new Complex(Wc,0) / poles[i]; //  hp_S = Wc / lp_S;

                }
            }

            // Init with zeros, no non-zero values to convert
            numZeros = numPoles;
            for (int i = 0; i < numZeros; i++)
            {
                zeros[i] = new Complex(0,0);
            }
        }


        //******************************************************************************
        // Convert lowpass poles to bandpass
        // use:  bp_S = 0.5 * lp_S * BW +
        //				   0.5 * sqrt ( BW^2 * lp_S^2 - 4*Wc^2 )
        // where   BW = W2 - W1
        //		    Wc^2 = W2 * W1
        //******************************************************************************

        static void convert2bandpass()
        {
            bw = f2 - f1;
            Wc = Math.Sqrt(f1 * f2);

            // Calculate bandpass gain
            gain *= Math.Pow(bw, numPoles - numZeros);

            // Convert LP poles to BP: these two sets of for-loops result in an ordered
            // list of poles and their complex conjugates
            List<Complex> tempPoles = new List<Complex>();
            for (int i = 0; i < numPoles; i++)
            {    // First set of poles + conjugates
                if (poles[i].Magnitude!=0)
                {
                    Complex firstterm = 0.5 * poles[i] * bw;
                    Complex secondterm = 0.5 * Complex.Sqrt((bw * bw) * (poles[i] * poles[i]) - 4 * Wc * Wc);
                    tempPoles.Add(firstterm + secondterm);
                }
            }

            for (int i = 0; i < numPoles; i++)
            {    // Second set of poles + conjugates
                if (poles[i].Magnitude!=0)
                {
                    Complex firstterm = 0.5 * poles[i] * bw;
                    Complex secondterm = 0.5 * Complex.Sqrt((bw * bw) * (poles[i] * poles[i]) - 4 * Wc * Wc);
                    tempPoles.Add(firstterm - secondterm);      // complex conjugate
                }
            }

            // Init zeros, no non-zero values to convert
            numZeros = numPoles;
            for (int i = 0; i < numZeros; i++)
            {
                zeros[i] = new Complex(0.0,0);
            }

            // Copy converted poles to output array
            int index = 0;
            foreach(Complex pole in tempPoles)
            {
                poles[index] = pole;
                index++;
            }
            numPoles = (int)tempPoles.Count;
        }


        //******************************************************************************
        // Convert lowpass poles to bandstop
        // use:  bs_S = 0.5 * BW / lp_S +
        //				   0.5 * sqrt ( BW^2 / lp_S^2 - 4*Wc^2 )
        // where   BW = W2 - W1
        //		 Wc^2 = W2 * W1
        //******************************************************************************

        static void convert2bandstop()
        {
            bw = f2 - f1;
            Wc = Math.Sqrt(f1 * f2);

            // Compute gain
            Complex prodz = new Complex(1.0, 0.0);
            Complex prodp = new Complex(1.0, 0.0);
            for (int i = 0; i < numZeros; i++)
            {
                prodz *= -zeros[i];
            }

            for (int i = 0; i < numPoles; i++)
            {
                prodp *= -poles[i];
            }

            gain *= prodz.Real / prodp.Real;

            // Convert LP zeros to band stop
            numZeros = numPoles;
            List<Complex> ztmp = new List<Complex>();
            for (int i = 0; i < numZeros; i++)
            {
                ztmp.Add(new Complex(0.0, Wc));
                ztmp.Add(new Complex(0.0, -Wc));         // complex conjugate
            }

            List<Complex> tempPoles = new List<Complex>();
            for (int i = 0; i < numPoles; i++)
            {    // First set of poles + conjugates
                if (poles[i].Magnitude != 0)
                {
                    Complex term1 = 0.5 * bw / poles[i];
                    Complex term2 = 0.5 * Complex.Sqrt((bw * bw) / (poles[i] * poles[i]) - (4 * Wc * Wc));
                    tempPoles.Add(term1 + term2);
                }
            }

            for (int i = 0; i < numPoles; i++)
            {    // Second set of poles + conjugates
                if (poles[i].Magnitude != 0)
                {
                    Complex term1 = 0.5 * bw / poles[i];
                    Complex term2 = 0.5 * Complex.Sqrt((bw * bw) / (poles[i] * poles[i]) - (4 * Wc * Wc));
                    tempPoles.Add(term1 - term2);       // complex conjugate
                }
            }

            // Copy converted zeros to output array
            int index = 0;
            foreach(Complex zero in ztmp)
            {
                zeros[index] = zero;
                index++;
            }
            numZeros = (int)ztmp.Count;

            // Copy converted poles to output array
            index = 0;
            foreach(Complex pole in tempPoles)
            {
                poles[index] = pole;
                index++;
            }
            numPoles = (int)tempPoles.Count;
        }
    }
}
