export class SoundPlayer {
  private readonly context = new AudioContext();
  private bleep: AudioBuffer;
  private blop: AudioBuffer;

  async initialize() {
    const bleepPromise = window.fetch("/audio/bleep.mp3").then(async (response) => {
      const arrayBuffer = await response.arrayBuffer();
      this.bleep = await this.context.decodeAudioData(arrayBuffer);
    });
    const blopPromise = window.fetch("/audio/blop.mp3").then(async (response) => {
      const arrayBuffer = await response.arrayBuffer();
      this.blop = await this.context.decodeAudioData(arrayBuffer);
    });

    return Promise.all([bleepPromise, blopPromise]);
  }

  playBleep() {
    if (!this.bleep) {
      throw "Buffer not initialized. Cannot play sound.";
    }

    this.play(this.bleep);
  }

  playBlop() {
    if (!this.blop) {
      throw "Buffer not initialized. Cannot play sound.";
    }

    this.play(this.blop);
  }

  private play(audioBuffer: AudioBuffer) {
    const source = this.context.createBufferSource();
    source.buffer = audioBuffer;
    source.connect(this.context.destination);
    source.start();
  }
}
