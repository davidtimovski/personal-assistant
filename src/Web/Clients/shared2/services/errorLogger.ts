export class ErrorLogger {
  constructor(private readonly application: string) {}

  logError(error: any) {
    // TODO
    window.console.error(error);
  }

  release() {}
}
