export class Formatter {
	static number(value: any, currency: string) {
		if (isNaN(parseFloat(value))) {
			return null;
		}

		if (currency === 'MKD') {
			return new Intl.NumberFormat('mk-MK', {
				maximumFractionDigits: 0
			}).format(value);
		}

		return new Intl.NumberFormat().format(value);
	}

	static money(value: any, currency: string) {
		if (isNaN(parseFloat(value))) {
			return null;
		}

		if (currency === 'MKD') {
			const formatted = new Intl.NumberFormat('mk-MK', {
				maximumFractionDigits: 0
			}).format(value);
			return formatted + ' MKD';
		}

		return new Intl.NumberFormat('de-DE', {
			style: 'currency',
			currency: currency
		}).format(value);
	}

	static moneyPrecise(value: any, currency: string) {
		if (isNaN(parseFloat(value))) {
			return null;
		}

		if (currency === 'MKD') {
			const formatted = new Intl.NumberFormat('mk-MK', {
				maximumFractionDigits: 4
			}).format(value);
			return formatted;
		}

		return new Intl.NumberFormat('de-DE', {
			style: 'currency',
			maximumFractionDigits: 4,
			currency: currency
		}).format(value);
	}
}
