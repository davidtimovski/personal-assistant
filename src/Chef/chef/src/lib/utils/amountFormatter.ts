export class AmountFormatter {
	static formatGrams(value: any): string | null {
		if (isNaN(parseFloat(value))) {
			return null;
		}

		return new Intl.NumberFormat().format(value);
	}

	static formatMilligrams(value: any): string | null {
		if (isNaN(parseFloat(value))) {
			return null;
		}

		if (value > 10) {
			return Math.round(value).toString();
		}

		return new Intl.NumberFormat().format(value);
	}
}
