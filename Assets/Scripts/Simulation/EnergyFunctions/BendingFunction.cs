﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Simulation.EnergyFunctions
{

    class BendingFunction : EnergyFunction
    {
        private int i0, i1, i2, i3;
        private int[] particles;
        private float invM0, invM1, invM2, invM3;
        private float[] inverseMasses;

        private Matrix4x4 Q;



        private BendingFunction(int i0, int i1, int i2, int i3, float invM0, float invM1, float invM2, float invM3, Matrix4x4 Q) {
            this.i0 = i0;
            this.i1 = i1;
            this.i2 = i2;
            this.i3 = i3;

            this.particles = new int[] { i0, i1, i2, i3 };

            this.invM0 = invM0;
            this.invM1 = invM1;
            this.invM2 = invM2;
            this.invM3 = invM3;

            this.inverseMasses = new float[] { invM0, invM1, invM2, invM3 };

            this.Q = Q;

        }

        /// <summary>
        /// Creates a bending function such that the current bending between the four given points is preserved. 
        /// The four given points are expected to form a stencil such that we have two triangles i0, i1, i2 and i0, i1, i3.
        /// Then the bending energy is used to keep i2 and i3 at the same angle with the edge i0i1 as the origin. 
        /// 
        ///     i2
        ///    /  \
        ///  i0 -- i1
        ///    \  /
        ///     i3
        /// 
        /// </summary> 
        public static BendingFunction create(ParticleModel pm, int i0, int i1, int i2, int i3)
        {
            float invM0 = pm.inverseMasses[i0];
            float invM1 = pm.inverseMasses[i1];
            float invM2 = pm.inverseMasses[i2];
            float invM3 = pm.inverseMasses[i3];

            Matrix4x4 Q = LocalHessianEnergy(i0, i1, i2, i3, ref pm.positions);

            return new BendingFunction(i0, i1, i2, i3, invM0, invM1, invM2, invM3, Q);
        }


        public int[] GetParticles()
        {
            return this.particles;
        }

        private static float TriangleArea(int i0, int i1, int i2, ref Vector3[] positions)
        {
            return 0.5f * Vector3.Cross(positions[i1] - positions[i0], positions[i2] - positions[i0]).magnitude;
        }

        /// <summary>
        /// Given a Vector4 V this method computes V^T * V. 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns>The result of transposing the given vector, and then multiplying that by the given vector. </returns>
        private static float InnerProduct(Vector4 vector)
        {
            return vector.w * vector.w + vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
        }

        /// <summary>
        /// Given a Vector3 V this method computes V^T * V. 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns>The result of transposing the given vector, and then multiplying that by the given vector. </returns>
        private static float InnerProduct(Vector3 vector)
        {
            return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
        }

        /// <summary>
        /// Compute the Hessian energy for this bending function for the given positions.
        /// </summary>
        /// <param name="positions">The positions of all particles, which are to be used to define area and the likes</param>
        /// <returns>The Hessian energy for the particles given the provided positions.</returns>
        private static Matrix4x4 LocalHessianEnergy(int i0, int i1, int i2, int i3, ref Vector3[] positions)
        {
            float area0 = TriangleArea(i0, i1, i2, ref positions);
            float area1 = TriangleArea(i0, i1, i3, ref positions);

            Vector3 e0 = positions[i1] - positions[i0];
            Vector3 e1 = positions[i2] - positions[i1];
            Vector3 e2 = positions[i0] - positions[i2];
            Vector3 e3 = positions[i3] - positions[i0];
            Vector3 e4 = positions[i1] - positions[i3];

            float C01 = 1 / (float) Math.Tan(Vector3.Angle(e0, e1));
            float C02 = 1 / (float) Math.Tan(Vector3.Angle(e0, e2));
            float C03 = 1 / (float) Math.Tan(Vector3.Angle(e0, e3));
            float C04 = 1 / (float) Math.Tan(Vector3.Angle(e0, e4));

            float factor = (3 / (area0 + area1));

            Vector4 K = new Vector4(C01 + C04, C02 + C03, -C01 - C02, -C03 - C04);
            Vector4 KT = new Vector4(K[3], K[2], K[1], K[0]);

            Matrix4x4 Q = new Matrix4x4();

            for (int i = 0; i < 4; ++i) {
                Q[i, i] = K[i] * KT[i];
                for (int j = 0; j < i; j++)
                    Q[i, j] = Q[j, i] = factor * K[i] * KT[j];
            }

            return Q;
        }

        public void solve(ref Vector3[] positions, ref Vector3[] corrections)
        {
            
        }
    }
}
