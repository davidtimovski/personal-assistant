export class TimeFormatter {
	static format(value: string, hoursLetter: string, minutesLetter: string): string | null {
		const hours = parseInt(value.substring(0, 2), 10);
		const minutes = parseInt(value.substring(3, 5), 10);

		if (hours === 0) {
			return minutes + minutesLetter;
		} else if (minutes === 0) {
			return hours + hoursLetter;
		}

		return hours + hoursLetter + ' ' + minutes + minutesLetter;
	}
}
