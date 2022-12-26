<script lang="ts">
  export let type: string;
  export let message: string;

  let iconClass: string;
  switch (type) {
    case "info":
      iconClass = "fa-info-circle";
      break;
    case "warning":
    case "danger":
      iconClass = "fa-exclamation-triangle";
      break;
  }
</script>

<div class="message {type}">
  <i class="fas {iconClass}" />
  <span contenteditable="false" bind:innerHTML={message} />
</div>

<style lang="scss">
  .message {
    border-radius: var(--border-radius);
    padding: 7px 12px;
    margin-bottom: 20px;
    font-size: 1rem;
    line-height: 1.5rem;
    text-align: center;

    i {
      margin-right: 5px;
    }

    &.info {
      background: var(--primary-color-light);
      color: var(--primary-color-dark);

      i {
        color: var(--primary-color-dark);
      }
    }

    &.warning {
      background: var(--warning-color);
      color: var(--warning-color-dark);

      i {
        color: var(--warning-color-dark);
      }
    }

    &.danger {
      background: #ffe0d9;
      color: var(--danger-color-dark);

      i {
        color: var(--danger-color-dark);
      }
    }
  }
</style>
