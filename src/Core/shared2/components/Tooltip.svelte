<script lang="ts">
  import { onMount, onDestroy } from "svelte";
  import { slide } from "svelte/transition";

  import type { Tooltip } from "../models/tooltip";
  import { TooltipsService } from "../services/tooltipsService";

  import { t } from "$lib/localization/i18n";

  export let key: string;
  export let application: string;

  let tooltip: Tooltip | null = null;
  let isVisible = false;
  let isOpen = false;
  let isDismissed = false;

  let tooltipsService: TooltipsService;

  function toggleOpen() {
    isOpen = !isOpen;
  }

  async function dismiss() {
    if (!tooltip) {
      return;
    }

    isDismissed = true;
    await tooltipsService.toggleDismissed(tooltip.key, true);
  }

  onMount(async () => {
    tooltipsService = new TooltipsService(application);

    tooltip = await tooltipsService.getByKey(key);
    if (!tooltip.isDismissed) {
      tooltip.question = (<any>$t(`tooltips.${key}`)).question;
      tooltip.answer = (<any>$t(`tooltips.${key}`)).answer;
      isVisible = true;
    }
  });

  onDestroy(() => {
    tooltipsService?.release();
  });
</script>

<div class="tooltip" class:visible={isVisible}>
  {#if tooltip}
    <div class="question-wrap">
      <button
        type="button"
        on:click={toggleOpen}
        class="question"
        class:glow={!isOpen}>{tooltip.question}</button
      >
    </div>

    {#if isOpen}
      <div in:slide class="answer-wrap">
        <div class="answer">
          <i class="fas fa-info-circle" class:faded={isDismissed} />
          <span>{tooltip.answer}</span>
          <div class="dismiss-wrap">
            <button type="button" on:click={dismiss} disabled={isDismissed}
              >{$t("tooltips.okayUnderstood")}</button
            >
          </div>
        </div>
      </div>
    {/if}

    {#if isDismissed}
      <div in:slide class="dismissed-wrap">
        <div class="dismissed">
          <i class="fas fa-arrow-alt-circle-up" />
          <span>{$t("tooltips.tooltipWillNoLongerShow")}</span>
        </div>
      </div>
    {/if}
  {/if}
</div>

<style lang="scss">
  .tooltip {
    display: none;
    font-size: 1rem;

    &.visible {
      display: block;
    }

    .question-wrap {
      margin: 10px 0;
      text-align: right;
      color: var(--primary-color);
    }

    .question {
      background: transparent;
      border: none;
      outline: none;
      text-decoration: underline;
      cursor: pointer;

      &.glow {
        animation: glow 1s ease-in-out infinite alternate;
      }
    }

    .answer-wrap {
      background: #fff;
      border: 1px solid #ddd;
      border-radius: 8px;
      margin-bottom: 10px;
    }

    .answer,
    .dismissed {
      padding: 10px 15px;
      line-height: 1.5rem;
    }

    .fa-info-circle,
    .fa-arrow-alt-circle-up {
      margin-right: 4px;
      color: var(--primary-color);
    }

    .fa-info-circle.faded {
      color: var(--faded-color);
    }

    .answer .dismiss-wrap {
      margin: 10px 0 3px;
      text-align: right;

      button {
        background: linear-gradient(225deg, #00a6ed, #7a46f3);
        background-size: 400% 400%;
        border: none;
        border-radius: var(--border-radius);
        outline: none;
        box-shadow: var(--box-shadow);
        padding: 0 12px;
        font-size: 1rem;
        line-height: 31px;
        color: #fff;

        animation: AnimateGradiant 8s ease infinite;
        transition: border-radius var(--transition);

        &:disabled {
          opacity: 0.6;
        }
      }
    }

    .dismissed-wrap {
      background: #fff;
      border: 1px solid #ddd;
      border-radius: 8px;
      margin: 10px 0;
    }
  }

  /* DESKTOP */
  @media screen and (min-width: 1200px) {
    .tooltip {
      font-size: 1.2rem;
    }
  }

  /* Workaround for sticky :hover on mobile devices */
  .touch-device .tooltip .question:hover {
    color: var(--primary-color);
  }
</style>
