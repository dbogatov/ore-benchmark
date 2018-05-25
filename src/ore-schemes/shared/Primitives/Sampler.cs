using System;

namespace ORESchemes.Shared.Primitives
{
	using RR = Decimal;

	public class SamplerFactory
	{
		/// <summary>
		/// Returns an initialized instance of a ISampler that works on 64 bits integers
		/// </summary>
		public static ISampler<ulong> GetSampler(byte[] entropy = null)
		{
			return new CustomSampler(entropy);
		}

		public static ISampler<ulong> GetSampler(IPRG prg)
		{
			return new CustomSampler(prg);
		}
	}

	public interface ISampler<T> where T : struct
	{
		T HyperGeometric(T population, T successes, T samples);
		T Uniform(T from, T to);
	}

	public class CustomSampler : ISampler<ulong>
	{
		private IPRG _generator;

		public CustomSampler(byte[] entropy = null)
		{
			_generator = PRGFactory.GetPRG(entropy);
		}

		public CustomSampler(IPRG prg)
		{
			_generator = prg;
		}

		// https://github.com/mathnet/mathnet-numerics
		public ulong HyperGeometric(ulong population, ulong successes, ulong samples)
		{
			if (population >= 100)
			{
				return EfficientHG(population, successes, samples);
			}
			else
			{
				return NaiveHG(population, successes, samples);
			}
		}

		public ulong Uniform(ulong from, ulong to)
		{
			ulong diff = to - from;

			long start = 0 - (long)(diff / 2);
			long end = 0 + (long)(diff / 2);

			long result = _generator.NextLong(start, end);

			return from + (ulong)(result - start);
		}

		private ulong NaiveHG(ulong population, ulong successes, ulong samples)
		{
			ulong x = 0;

			do
			{
				var p = (double)successes / population;
				var r = _generator.NextDouble(0, 1);
				if (r < p)
				{
					x++;
					successes--;
				}

				population--;
				samples--;
			}
			while (0 < samples);

			return x;
		}

		/// <summary>
		/// This function is borrowed from CryptDB implementation.
		/// https://github.com/CryptDB/cryptdb/blob/7678bc98d3054f1418371779c6d1050cd1a88b2e/crypto/hgd.cc
		/// 
		/// To preserve as much of the original code as possible, all subrotines and types were aliased with the method.
		/// The behavior of these methods is verified by examining NTL source code.
		/// 
		/// I make assumption that is sqr of negative number is invoked (possible), then sqr uses the absolute value 
		/// of the input.
		/// 
		/// It is empirically tested that the functions runs without errors (bit overflows) for parameters
		/// UInt64.MaxValue / 100, (ulong)UInt32.MaxValue, (ulong)UInt32.MaxValue
		/// </summary>
		private ulong EfficientHG(ulong population, ulong successes, ulong samples)
		{
			Func<ulong, RR> to_RR = (value) => Convert.ToDecimal(value);
			Func<RR, RR> exp = (value) => Convert.ToDecimal(Math.Exp((double)value));
			Func<RR, RR> log = (value) => Convert.ToDecimal(Math.Log((double)value));
			Func<RR, RR> round = (value) => Math.Round(value, MidpointRounding.ToEven);
			Func<RR, long> to_int = (value) => Convert.ToInt64(value);
			Func<RR> RAND = () => Convert.ToDecimal(_generator.NextDouble(0, 1));
			Func<RR, RR> sqr = (value) => Convert.ToDecimal(Math.Sqrt((double) Math.Abs(value)));
			Func<RR, RR> trunc = (value) => Math.Truncate(value);
			Func<RR, ulong> to_ZZ = (value) => Convert.ToUInt64(value);
			Action<bool> throw_c = (value) => throw new ArgumentException();


			Func<decimal, decimal> AFC = (I) =>
			{
				/*
				* FUNCTION TO EVALUATE LOGARITHM OF THE FACTORIAL I
				* IF (I .GT. 7), USE STIRLING'S APPROXIMATION
				* OTHERWISE,  USE TABLE LOOKUP
				*/
				double[] AL = new double[8] { 0.0, 0.0, 0.6931471806, 1.791759469, 3.178053830, 4.787491743, 6.579251212, 8.525161361 };

				if (I <= 7)
				{
					return (decimal)AL[to_int(round(I))];
				}
				else
				{
					RR LL = log(I);
					return (I + 0.5M) * LL - I + 0.399089934M;
				}
			};

			ulong KK = samples;
			ulong NN1 = successes;
			ulong NN2 = population - successes;

			RR JX;      // the result
			RR TN, N1, N2, K;
			RR P, U, V, A, IX, XL, XR, M;
			RR KL, KR, LAMDL, LAMDR, NK, NM, P1, P2, P3;

			bool REJECT;
			RR MINJX, MAXJX;

			decimal CON = 57.56462733M;
			decimal DELTAL = 0.0078M;
			decimal DELTAU = 0.0034M;
			decimal SCALE = 1.0e25M;

			// /*
			//  * CHECK PARAMETER VALIDITY
			//  */
			if ((NN1 < 0) || (NN2 < 0) || (KK < 0) || (KK > NN1 + NN2))
				throw_c(false);

			/*
			 * INITIALIZE
			 */
			REJECT = true;

			if (NN1 >= NN2)
			{
				N1 = to_RR(NN2);
				N2 = to_RR(NN1);
			}
			else
			{
				N1 = to_RR(NN1);
				N2 = to_RR(NN2);
			}

			TN = N1 + N2;

			if (to_RR(KK + KK) >= TN)
			{
				K = TN - to_RR(KK);
			}
			else
			{
				K = to_RR(KK);
			}

			M = (K + 1) * (N1 + 1) / (TN + 2);

			if (K - N2 < 0)
			{
				MINJX = 0;
			}
			else
			{
				MINJX = K - N2;
			}

			if (N1 < K)
			{
				MAXJX = N1;
			}
			else
			{
				MAXJX = K;
			}

			/*
			 * GENERATE RANDOM VARIATE
			 */
			if (MINJX == MAXJX)
			{
				/*
				 * ...DEGENERATE DISTRIBUTION...
				 */
				IX = MAXJX;
			}
			else if (M - MINJX < 10)
			{
				/*
				 * ...INVERSE TRANSFORMATION...
				 * Shouldn't really happen in OPE because M will be on the order of N1.
				 * In practice, this does get invoked.
				 */
				RR W;
				if (K < N2)
				{
					W = exp(CON + AFC(N2) + AFC(N1 + N2 - K) - AFC(N2 - K) - AFC(N1 + N2));
				}
				else
				{
					W = exp(CON + AFC(N1) + AFC(K) - AFC(K - N2) - AFC(N1 + N2));
				}

			label10:
				P = W;
				IX = MINJX;
				U = RAND() * SCALE;

			label20:
				if (U > P)
				{
					U = U - P;
					P = P * (N1 - IX) * (K - IX);
					IX = IX + 1;
					P = P / IX / (N2 - K + IX);
					if (IX > MAXJX)
						goto label10;
					goto label20;
				}
			}
			else
			{
				/*
				 * ...H2PE...
				 */
				// RR S = sqr((TN - K) * K * N1 * N2 / (TN - 1) / TN / TN);
				RR S =
					sqr(((TN - K) / (TN - 1))) *
					sqr((K / TN)) *
					sqr((N2 / TN)) *
					sqr(N1)
				;

				/*
				 * ...REMARK:  D IS DEFINED IN REFERENCE WITHOUT INT.
				 * THE TRUNCATION CENTERS THE CELL BOUNDARIES AT 0.5
				 */
				RR D = trunc(1.5M * S) + 0.5M;
				XL = trunc(M - D + 0.5M);
				XR = trunc(M + D + 0.5M);
				A = AFC(M) + AFC(N1 - M) + AFC(K - M) + AFC(N2 - K + M);
				RR expon = A - AFC(XL) - AFC(N1 - XL) - AFC(K - XL) - AFC(N2 - K + XL);

				KL = exp(expon);
				KR = exp(A - AFC(XR - 1) - AFC(N1 - XR + 1) - AFC(K - XR + 1) - AFC(N2 - K + XR - 1));
				LAMDL = -log(XL * (N2 - K + XL) / (N1 - XL + 1) / (K - XL + 1));
				LAMDR = -log((N1 - XR + 1) * (K - XR + 1) / XR / (N2 - K + XR));
				P1 = 2 * D;
				P2 = P1 + KL / LAMDL;
				P3 = P2 + KR / LAMDR;

			label30:
				U = RAND() * P3;
				V = RAND();

				if (U < P1)
				{
					/* ...RECTANGULAR REGION... */
					IX = XL + U;
				}
				else if (U <= P2)
				{
					/* ...LEFT TAIL... */
					IX = XL + log(V) / LAMDL;
					if (IX < MINJX)
					{
						goto label30;
					}
					V = V * (U - P1) * LAMDL;
				}
				else
				{
					/* ...RIGHT TAIL... */
					IX = XR - log(V) / LAMDR;
					if (IX > MAXJX)
					{
						goto label30;
					}
					V = V * (U - P2) * LAMDR;
				}

				/*
				 * ...ACCEPTANCE/REJECTION TEST...
				 */
				RR F;
				if ((M < 100) || (IX <= 50))
				{
					/* ...EXPLICIT EVALUATION... */
					F = 1.0M;
					if (M < IX)
					{
						for (RR I = M + 1; I < IX; I++)
						{
							/*40*/
							F = F * (N1 - I + 1) * (K - I + 1) / (N2 - K + I) / I;
						}
					}
					else if (M > IX)
					{
						for (RR I = IX + 1; I < M; I++)
						{
							/*50*/
							F = F * I * (N2 - K + I) / (N1 - I) / (K - I);
						}
					}
					if (V <= F)
					{
						REJECT = false;
					}
				}
				else
				{
					/* ...SQUEEZE USING UPPER AND LOWER BOUNDS... */

					RR Y = IX;
					RR Y1 = Y + 1.0M;
					RR YM = Y - M;
					RR YN = N1 - Y + 1.0M;
					RR YK = K - Y + 1.0M;
					NK = N2 - K + Y1;
					RR R = -YM / Y1;
					S = YM / YN;
					RR T = YM / YK;
					RR E = -YM / NK;
					RR G = YN * YK / (Y1 * NK) - 1.0M;
					RR DG = 1.0M;
					if (G < 0) { DG = 1.0M + G; }
					RR GU = G * (1.0M + G * (-0.5M + G / 3.0M));
					RR GL = GU - 0.25M * sqr(sqr(G)) / DG;
					RR XM = M + 0.5M;
					RR XN = N1 - M + 0.5M;
					RR XK = K - M + 0.5M;
					NM = N2 - K + XM;
					RR UB = Y * GU - M * GL + DELTAU +
						XM * R * (1.0M + R * (-.5M + R / 3.0M)) +
						XN * S * (1.0M + S * (-.5M + S / 3.0M)) +
						XK * T * (1.0M + T * (-.5M + T / 3.0M)) +
						NM * E * (1.0M + E * (-.5M + E / 3.0M));

					/* ...TEST AGAINST UPPER BOUND... */

					RR ALV = log(V);
					if (ALV > UB)
					{
						REJECT = true;
					}
					else
					{
						/* ...TEST AGAINST LOWER BOUND... */

						RR DR = XM * sqr(sqr(R));
						if (R < 0)
						{
							DR = DR / (1.0M + R);
						}
						RR DS = XN * sqr(sqr(S));
						if (S < 0)
						{
							DS = DS / (1.0M + S);
						}
						RR DT = XK * sqr(sqr(T));
						if (T < 0)
						{
							DT = DT / (1.0M + T);
						}
						RR DE = NM * sqr(sqr(E));
						if (E < 0)
						{
							DE = DE / (1.0M + E);
						}
						if (ALV < UB - 0.25M * (DR + DS + DT + DE) + (Y + M) * (GL - GU) - DELTAL)
						{
							REJECT = false;
						}
						else
						{
							/* ...STIRLING'S FORMULA TO MACHINE ACCURACY... */

							if (ALV <=
								(A - AFC(IX) -
								 AFC(N1 - IX) - AFC(K - IX) - AFC(N2 - K + IX)))
							{
								REJECT = false;
							}
							else
							{
								REJECT = true;
							}
						}
					}
				}
				if (REJECT)
				{
					goto label30;
				}
			}

			/*
			 * RETURN APPROPRIATE VARIATE
			 */

			if (KK + KK >= to_ZZ(TN))
			{
				if (NN1 > NN2)
				{
					IX = to_RR(KK - NN2) + IX;
				}
				else
				{
					IX = to_RR(NN1) - IX;
				}
			}
			else
			{
				if (NN1 > NN2)
				{
					IX = to_RR(KK) - IX;
				}
			}
			JX = IX;
			return to_ZZ(JX);
		}
	}
}
