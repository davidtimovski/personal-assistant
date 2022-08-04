<script lang="ts">
	export let type: string;
	export let message: string;

	let iconClass = type === 'warning' ? 'fa-exclamation-triangle' : 'fa-info-circle';
</script>

<div class="message {type}">
	<i class="fas {iconClass}" />
	<span>{message}</span>
</div>

<style lang="scss">
	.message {
		border-radius: var(--border-radius);
		padding: 7px 12px;
		margin-bottom: 20px;
		font-size: 1rem;
		line-height: 1.5rem;

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
	}
</style>
