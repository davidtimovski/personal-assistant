CREATE TABLE public.accountant_transactions
(
    id serial NOT NULL,
    from_account_id integer,
    to_account_id integer,
    category_id integer,
    amount decimal(10, 2) NOT NULL,
    from_stocks decimal(10, 4),
    to_stocks decimal(10, 4),
    currency character varying(3) COLLATE pg_catalog."default" NOT NULL DEFAULT 'EUR',
    description character varying(500) COLLATE pg_catalog."default",
    date date NOT NULL,
    is_encrypted boolean NOT NULL DEFAULT FALSE,
    encrypted_description bytea,
    salt bytea,
    nonce bytea,
    encryption_hint character varying(100) COLLATE pg_catalog."default",
	generated boolean NOT NULL DEFAULT FALSE,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_accountant_transactions PRIMARY KEY (id),
    CONSTRAINT fk_accountant_transactions_accountant_accounts_to_account_id FOREIGN KEY (to_account_id)
    REFERENCES public.accountant_accounts (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_accountant_transactions_accountant_accounts_from_account_id FOREIGN KEY (from_account_id)
    REFERENCES public.accountant_accounts (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_accountant_transactions_accountant_categories_category_id FOREIGN KEY (category_id)
    REFERENCES public.accountant_categories (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.accountant_transactions
    OWNER to personalassistant;

CREATE INDEX ix_accountant_transactions_from_account_id
    ON public.accountant_transactions USING btree
    (from_account_id)
    TABLESPACE pg_default;

CREATE INDEX ix_accountant_transactions_to_account_id
    ON public.accountant_transactions USING btree
    (to_account_id)
    TABLESPACE pg_default;
    
CREATE INDEX ix_accountant_transactions_category_id
    ON public.accountant_transactions USING btree
    (category_id)
    TABLESPACE pg_default;
