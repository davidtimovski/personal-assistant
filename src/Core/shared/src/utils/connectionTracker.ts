export class ConnectionTracker {
  public isOnline = navigator.onLine;

  constructor() {
    const updateIsOnline = () => {
      this.isOnline = navigator.onLine;
    };
    window.addEventListener("online", updateIsOnline);
    window.addEventListener("offline", updateIsOnline);
  }
}
