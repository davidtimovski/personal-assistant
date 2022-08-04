<script lang="ts">
	import { onMount } from 'svelte/internal';

	import { AlertStatus } from '../../../../shared2/models/enums/alertEvents';

	import { t } from '$lib/localization/i18n';
	import { alertState } from '$lib/stores';

	let type: string | null;
	let message: string | null;
	let refreshLink: HTMLAnchorElement | null;
	let shown = false;
	let hideTimeout = 0;
	let resetMessageTimeout = 0;

	function show(alertType: string, alertMessage: string) {
		if (resetMessageTimeout) {
			window.clearTimeout(resetMessageTimeout);
			resetMessageTimeout = 0;
		}

		type = alertType;
		message = alertMessage;
		shown = true;
	}

	function showTemporary(type: string, message: string) {
		show(type, message);

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
		(<HTMLAnchorElement>refreshLink).style.display = 'none';
	}

	function refresh() {
		window.location.reload();
	}

	onMount(() => {
		refreshLink = <HTMLAnchorElement>document.getElementById('refresh-link');

		alertState.subscribe((value) => {
			if (value.status === AlertStatus.Error) {
				let message = '';

				if (value.messages.length > 0) {
					message = value.messages.join('<br>');
				} else if (value.messageKey) {
					const translationKey = value.messageKey;

					if (refreshLink) {
						refreshLink.style.display = translationKey === 'unexpectedError' ? 'block' : 'none';
					}

					message = $t(translationKey);
				}

				show('error', message);
			} else if (value.status === AlertStatus.Success) {
				showTemporary('success', $t(<string>value.messageKey));
			} else if (type === 'error') {
				hide();
			}
		});
	});
</script>

<div on:click={hide} class="alert {type}" class:shown>
	<span class="alert-body">
		<span class="alert-message" contenteditable="true" bind:innerHTML={message} />
		<a id="refresh-link" on:click={refresh} class="refresh-link">{$t('refresh')}</a>
	</span>
</div>

<style lang="scss">
	.alert {
		position: fixed;
		bottom: -45px;
		z-index: 1;
		width: 100%;
		text-align: center;
		font-size: 1rem;
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

			.refresh-link {
				display: none;
				margin-top: 10px;
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
