<script lang="ts">
  import { onDestroy } from "svelte";

  import { AlertStatus } from "../models/enums/alertEvents";

  import { t } from "$lib/localization/i18n";
  import { alertState } from "$lib/stores";

  let type: string | null;
  let message: string | null;
  let refreshButtonVisible = false;
  let shown = false;
  let hideTimeout = 0;
  let resetMessageTimeout = 0;

  const unsubscriber = alertState.subscribe((value) => {
    if (value.status === AlertStatus.Error) {
      let message = "";
      let showRefreshLink = false;

      if (value.messages.length > 0) {
        message = value.messages.join("<br>");
      } else if (value.messageKey) {
        const translationKey = value.messageKey;

        showRefreshLink = translationKey === "unexpectedError";

        message = $t(translationKey);
      }

      showError(message, showRefreshLink);
    } else if (value.status === AlertStatus.Success) {
      showSuccess($t(<string>value.messageKey));
    } else {
      shown = false;
    }
  });

  function showError(alertMessage: string, showRefreshLink: boolean) {
    if (resetMessageTimeout) {
      window.clearTimeout(resetMessageTimeout);
      resetMessageTimeout = 0;
    }

    type = "error";
    message = alertMessage;
    refreshButtonVisible = showRefreshLink;
    shown = true;
  }

  function showSuccess(alertMessage: string) {
    if (resetMessageTimeout) {
      window.clearTimeout(resetMessageTimeout);
      resetMessageTimeout = 0;
    }

    type = "success";
    message = alertMessage;
    refreshButtonVisible = false;
    shown = true;

    if (hideTimeout) {
      window.clearTimeout(hideTimeout);
      hideTimeout = 0;
    }

    hideTimeout = window.setTimeout(() => {
      hide();
    }, 5000);
  }

  function hide() {
    shown = false;

    resetMessageTimeout = window.setTimeout(() => {
      reset();
    }, 1000);

    alertState.update((x) => {
      x.hide();
      return x;
    });
  }

  function reset() {
    type = null;
    message = null;
    refreshButtonVisible = false;
  }

  function refresh() {
    window.location.reload();
  }

  onDestroy(unsubscriber);
</script>

<div class="alert {type}" class:shown>
  <span class="alert-body">
    <div
      class="alert-message"
      contenteditable="true"
      bind:innerHTML={message}
    />

    {#if refreshButtonVisible}
      <button type="button" on:click={refresh} class="refresh-button"
        >{$t("refresh")}</button
      >
    {/if}
  </span>
</div>

<style lang="scss">
  .alert {
    position: fixed;
    bottom: -45px;
    z-index: 1;
    width: calc(100% - 30px);
    padding: 0 15px;
    text-align: center;
    font-size: 1rem;
    line-height: 1.6rem;
    transition: bottom 700ms ease-in-out, top 700ms ease-in-out;

    &.shown {
      bottom: 30px;
    }

    .alert-body {
      display: inline-block;
      border-radius: 6px;
      box-shadow: var(--box-shadow);
      padding: 7px 15px;
      text-align: center;
      user-select: none;

      .refresh-button {
        background: transparent;
        border: none;
        outline: none;
        text-decoration: underline;
      }
    }

    &.error {
      .alert-message {
        color: var(--danger-color);
      }

      .alert-body {
        background: #fdd;
      }
    }

    &.success {
      .alert-message {
        color: var(--green-color-dark);
      }

      .alert-body {
        background: #dfd;
      }
    }
  }

  @media screen and (min-width: 1200px) {
    .alert {
      top: -50px;
      bottom: unset;
      right: 30px;
      width: unset;
      max-width: 400px;
      font-size: 1.1rem;
      text-align: right;

      &.shown {
        top: 30px;
      }

      .alert-body {
        padding: 10px 15px;
      }
    }
  }
</style>
