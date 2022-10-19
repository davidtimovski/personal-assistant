CREATE TABLE public.accountant_automatic_transactions
(
    id serial NOT NULL,
    user_id integer NOT NULL,
	is_deposit boolean NOT NULL DEFAULT FALSE,
    category_id integer,
    amount decimal(10, 2) NOT NULL,
    currency character varying(3) COLLATE pg_catalog."default" NOT NULL DEFAULT 'EUR',
    description character varying(255) COLLATE pg_catalog."default",
	day_in_month smallint NOT NULL,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_accountant_automatic_transactions PRIMARY KEY (id),
    CONSTRAINT fk_accountant_automatic_transactions_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_acc_automatic_transactions_acc_categories_category_id FOREIGN KEY (category_id)
    REFERENCES public.accountant_categories (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.accountant_automatic_transactions
    OWNER to personalassistant;

CREATE INDEX ix_accountant_automatic_transactions_user_id
    ON public.accountant_automatic_transactions USING btree
    (user_id)
    TABLESPACE pg_default;

CREATE INDEX ix_accountant_automatic_transactions_category_id
    ON public.accountant_automatic_transactions USING btree
    (category_id)
    TABLESPACE pg_default;
