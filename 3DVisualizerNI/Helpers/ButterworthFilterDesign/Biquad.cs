namespace _3DVisualizerNI.Helpers.ButterworthFilterDesign
{
    public class Biquad
    {
        // Coefficients are public, as client classes and test harnesses need direct access.
        // Special accessors to better encapsulate data, would be less readable,
        // so skipping for pedagogical reasons. Another idea would be to make this a struct.
        //
        public double b0, b1, b2, a1, a2;  // second order section variable
        public double a3, a4, b3, b4;      // fourth order section variables

        public void DF2TFourthOrderSection(double B0, double B1, double B2, double B3, double B4,
                                    double A0, double A1, double A2, double A3, double A4)
        {
            b0 = B0 / A0;
            b1 = B1 / A0;
            b2 = B2 / A0;
            b3 = B3 / A0;
            b4 = B4 / A0;

            a1 = (-A1) / A0;  // The negation conforms the Direct-Form II Transposed discrete-time
            a2 = (-A2) / A0;  // filter (DF2T) coefficients to the expectations of the process function.
            a3 = (-A3) / A0;
            a4 = (-A4) / A0;
        }


        public void DF2TBiquad(double B0, double B1, double B2,
                                double A0, double A1, double A2)
        {
            b0 = B0 / A0;
            b1 = B1 / A0;
            b2 = B2 / A0;
            a1 = (-A1) / A0;  // The negation conforms the Direct-Form II Transposed discrete-time
            a2 = (-A2) / A0;  // filter (DF2T) coefficients to the expectations of the process function.
        }
    }

}
