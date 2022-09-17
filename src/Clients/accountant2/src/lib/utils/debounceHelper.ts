export class DebounceHelper {
	private static timer: number | undefined;

	static debounce = (callback: () => void, delay: number) => {
		clearTimeout(this.timer);

		this.timer = window.setTimeout(() => {
			callback();
		}, delay);
	};
}
