﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Wiimote3Point
{
    /// <summary>
    /// Represents a Wiimote in 3D space using a triangular sensor bar.
    /// Using the wiimote's IR camera picking up 3 points, plus the real world
    /// characteristics of the sensor triangle, the wiimote's position and
    /// orientation can calculated. That information is then sent in eventargs
    /// along with the full wiimote state.
    /// </summary>
    public class Wiimote3P
    {
        /// <summary>
        /// Event raised when Wiimote state is changed.
        /// </summary>
        public event EventHandler<Wiimote3PChangedEventArgs> Wiimote3PChanged;

        /// <summary>
        /// Full access to the underlying wiimote.
        /// </summary>
        public WiimoteLib.Wiimote wiimote;

        private SensorTriangle sensorTriangle;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sensorTriangle">The sensor triangle.</param>
        public Wiimote3P(SensorTriangle sensorTriangle)
        {
            this.sensorTriangle = sensorTriangle;
        }

        /// <summary>
        /// Connect to the wiimote.
        /// </summary>
        public void Connect()
        {
            wiimote.WiimoteChanged += wm_WiimoteChanged;
            wiimote.Connect();
            wiimote.SetReportType(WiimoteLib.InputReport.IRAccel, true);
            wiimote.SetLEDs(false, true, true, false);
        }

        private void wm_WiimoteChanged(object sender, WiimoteLib.WiimoteChangedEventArgs args)
        {
            if (args.WiimoteState.IRState.IRSensors.Length >= 3)
            {
                Vector<double> unitVectorf1 = GetUnitVector(args.WiimoteState.IRState.IRSensors[0].RawPosition);
                Vector<double> unitVectorf2 = GetUnitVector(args.WiimoteState.IRState.IRSensors[1].RawPosition);
                Vector<double> unitVectorf3 = GetUnitVector(args.WiimoteState.IRState.IRSensors[2].RawPosition);

                List<PositionOrientation> po = P3PMath.Solve(sensorTriangle.P1, sensorTriangle.P2, sensorTriangle.P3, unitVectorf1, unitVectorf2, unitVectorf3);
                // lazy for now just return first solution
                Wiimote3PChanged(this, new Wiimote3PChangedEventArgs(args.WiimoteState, po[0]));
            }
            else
            {

            }

        }

        private Vector<double> GetUnitVector(WiimoteLib.Point pixelcoords)
        {
            int PIXELS_X = 1024;
            int PIXELS_Y = 768;
            const double FOV_X = Math.PI / 4;

            double pxFromOriginX = (PIXELS_X/2) - pixelcoords.X;
            double pxFromOriginY = (PIXELS_Y/2) - pixelcoords.Y;
            double pxFromOriginZ = (1 / Math.Tan(FOV_X/2)) * PIXELS_X;
            Vector<double> unitVector = new DenseVector(new double[] { pxFromOriginX, pxFromOriginY, pxFromOriginZ });
            return unitVector.Normalize(2);
        }
    }

}