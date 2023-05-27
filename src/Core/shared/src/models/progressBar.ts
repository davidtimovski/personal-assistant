export class ProgressBar {
  public active = false;
  public progress = 0;
  public visible = false;
  public intervalId: number;

  public start() {
    this.active = true;
    this.progress = 10;

    this.intervalId = window.setInterval(() => {
      if (this.progress < 85) {
        this.progress += 15;
      } else {
        window.clearInterval(this.intervalId);
      }
    }, 500);

    this.visible = true;
  }

  public finish() {
    window.setTimeout(() => {
      this.progress = 100;
      this.active = false;
    }, 500);
  }
}
