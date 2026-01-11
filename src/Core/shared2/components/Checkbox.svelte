<script lang="ts">
	import { t } from '$lib/localization/i18n';

	let {
		value = $bindable<boolean | null>(),
		labelKey,
		disabled = false,
		onchange
	}: { value: boolean | null; labelKey?: string | null; disabled?: boolean; onchange?: any } = $props();
</script>

<!-- svelte-ignore a11y_label_has_associated_control -->
<label class="checkbox-toggle" class:disabled class:no-label={!labelKey}>
	{#if labelKey}
		<span>{$t(labelKey)}</span>
	{/if}

	<span class="checkbox-toggle-content">
		<input type="checkbox" bind:checked={value} {onchange} {disabled} />
		<svg class="is-checked" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 426.67 426.67">
			<path
				d="M153.504 366.84c-8.657 0-17.323-3.303-23.927-9.912L9.914 237.265c-13.218-13.218-13.218-34.645 0-47.863 13.218-13.218 34.645-13.218 47.863 0l95.727 95.727 215.39-215.387c13.218-13.214 34.65-13.218 47.86 0 13.22 13.218 13.22 34.65 0 47.863L177.435 356.928c-6.61 6.605-15.27 9.91-23.932 9.91z"
			/>
		</svg>
		<svg class="is-unchecked" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 212.982 212.982">
			<path
				d="M131.804 106.49l75.936-75.935c6.99-6.99 6.99-18.323 0-25.312-6.99-6.99-18.322-6.99-25.312 0L106.49 81.18 30.555 5.242c-6.99-6.99-18.322-6.99-25.312 0-6.99 6.99-6.99 18.323 0 25.312L81.18 106.49 5.24 182.427c-6.99 6.99-6.99 18.323 0 25.312 6.99 6.99 18.322 6.99 25.312 0L106.49 131.8l75.938 75.937c6.99 6.99 18.322 6.99 25.312 0 6.99-6.99 6.99-18.323 0-25.313l-75.936-75.936z"
				fill-rule="evenodd"
				clip-rule="evenodd"
			/>
		</svg>
	</span>
</label>

<style lang="scss">
	.checkbox-toggle {
		display: flex;
		justify-content: space-between;
		padding: 7px 4px 6px;
		line-height: 37px;
		cursor: pointer;
		transition: opacity var(--transition);

		&.no-label {
			display: inline-flex;
		}

		&.disabled {
			opacity: 0.5;
			cursor: default;

			input[type='checkbox'] {
				cursor: default;
			}
		}

		span {
			line-height: 29px;
			user-select: none;
		}

		&:not(.disabled):hover span {
			color: var(--primary-color);
		}

		&-content {
			display: block;
			width: 56px;
			height: 27px;
			position: relative;
			display: inline-block;

			input[type='checkbox'] {
				width: 56px;
				height: 27px;
				margin: 0;
				appearance: none;
				background: #bbc;
				border-radius: 3px;
				position: relative;
				outline: 0;
				cursor: pointer;
				transition: all 100ms;
			}

			input[type='checkbox']:after {
				position: absolute;
				content: '';
				top: 3px;
				left: 3px;
				width: 21px;
				height: 21px;
				background: #e5e5e5;
				z-index: 2;
				border-radius: 2px;
				transition: all 250ms;
			}

			svg {
				position: absolute;
				transform-origin: 50% 50%;
				fill: #fff;
				transition: all 250ms;
				z-index: 1;
				cursor: pointer;
			}

			.is-checked {
				width: 18px;
				top: 11px;
				left: 7px;
				transform: translateX(190%) translateY(-30%) scale(0);
			}

			.is-unchecked {
				width: 13px;
				top: 12px;
				right: 9px;
				transform: translateX(0) translateY(-30%) scale(1);
			}

			/* Checked State */
			input[type='checkbox']:checked {
				background: var(--primary-color);
			}

			input[type='checkbox']:checked:after {
				left: calc(100% - 24px);
			}

			input[type='checkbox']:checked + .is-checked {
				transform: translateX(0) translateY(-30%) scale(1);
			}

			input[type='checkbox']:checked ~ .is-unchecked {
				transform: translateX(-190%) translateY(-30%) scale(0);
			}
		}
	}

	/* Workaround for sticky :hover on mobile devices */
	.touch-device label.checkbox-toggle:not(.disabled):hover span {
		color: var(--regular-color);
	}
</style>
