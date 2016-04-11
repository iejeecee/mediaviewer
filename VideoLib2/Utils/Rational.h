#pragma once

namespace VideoLib2
{
	public ref class Rational
	{
	protected:
		int numerator;
		int denominator;
	public:

		Rational(int numerator, int denominator)
		{
			Numerator = numerator;
			Denominator = denominator;
		}

		property int Numerator
		{
			int get() {

				return numerator;
			}

			void set(int value)
			{
				numerator = value;
			}
		}


		property int Denominator
		{
			int get() {

				return denominator;
			}

			void set(int value)
			{
				denominator = value;
			}
		}

	};

}