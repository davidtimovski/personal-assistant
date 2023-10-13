export class Formatter {
	private static withFractions = new Set(['EUR', 'USD']);

	static number(value: any, culture: string | string[] | undefined, fractionDigits?: number | undefined) {
		if (isNaN(parseFloat(value))) {
			return '';
		}

		return new Intl.NumberFormat(culture, {
			maximumFractionDigits: fractionDigits ? fractionDigits : 0
		}).format(value);
	}

	static moneyWithoutCurrency(value: any, currency: string | null, culture: string | string[] | undefined) {
		if (isNaN(parseFloat(value)) || !currency || !culture) {
			return '';
		}

		return new Intl.NumberFormat(culture, {
			maximumFractionDigits: Formatter.withFractions.has(currency) ? 2 : 0
		}).format(value);
	}

	static numberPrecise(value: any, currency: string | null, culture: string | string[] | undefined, fractionDigits?: number | undefined) {
		if (isNaN(parseFloat(value)) || !currency || !culture) {
			return '';
		}

		return new Intl.NumberFormat(culture, {
			maximumFractionDigits: fractionDigits ? fractionDigits : 2
		}).format(value);
	}

	static money(value: any, currency: string | null, culture: string | string[] | undefined) {
		if (isNaN(parseFloat(value)) || !currency || !culture) {
			return '';
		}

		const fraction = Formatter.withFractions.has(currency) ? 2 : 0;

		const formatConfig = new Intl.NumberFormat(culture, {
			style: 'currency',
			currency: currency,
			currencyDisplay: 'narrowSymbol',
			minimumFractionDigits: fraction,
			maximumFractionDigits: fraction
		});

		return formatConfig.format(value);
	}

	static moneyPrecise(value: any, currency: string | null, culture: string | string[] | undefined, fractionDigits?: number | undefined) {
		if (isNaN(parseFloat(value)) || !currency || !culture) {
			return '';
		}

		return new Intl.NumberFormat(culture, {
			style: 'currency',
			currency: currency,
			minimumFractionDigits: fractionDigits,
			maximumFractionDigits: fractionDigits
		}).format(value);
	}
}
