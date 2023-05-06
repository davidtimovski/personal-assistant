export class ThrottleHelper {
	private static readonly delayMs = 350;

	/** Used to delay UI update if server responds too quickly */
	static executeAfterDelay(callback: () => void, startTime: number) {
		const timeTaken = Date.now() - startTime;
		const sleepTime = this.delayMs - timeTaken;

		window.setTimeout(() => {
			callback();
		}, Math.max(sleepTime, 0));
	}
}
