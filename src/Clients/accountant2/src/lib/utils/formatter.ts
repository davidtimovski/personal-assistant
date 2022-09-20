export class Formatter {
	static number(value: any, currency: string | null) {
		if (isNaN(parseFloat(value)) || !currency) {
			return '';
		}

		if (currency === 'MKD') {
			return new Intl.NumberFormat('mk-MK', {
				maximumFractionDigits: 0
			}).format(value);
		}

		return new Intl.NumberFormat().format(value);
	}

	static money(value: any, currency: string | null, fractionDigits?: number | undefined) {
		if (isNaN(parseFloat(value)) || !currency) {
			return '';
		}

		if (currency === 'MKD') {
			const formatted = new Intl.NumberFormat('mk-MK', {
				maximumFractionDigits: fractionDigits ? fractionDigits : 0
			}).format(value);
			return formatted + ' MKD';
		}

		return new Intl.NumberFormat('de-DE', {
			style: 'currency',
			currency: currency,
			maximumFractionDigits: fractionDigits
		}).format(value);
	}
}
