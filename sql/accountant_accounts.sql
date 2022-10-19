CREATE TABLE public.accountant_accounts
(
    id serial NOT NULL,
    user_id integer NOT NULL,
    name character varying(30) NOT NULL COLLATE pg_catalog."default",
    is_main boolean NOT NULL DEFAULT FALSE,
    currency character varying(3) COLLATE pg_catalog."default" NOT NULL DEFAULT 'EUR',
    stock_price decimal(10, 4),
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_accountant_accounts PRIMARY KEY (id),
    CONSTRAINT fk_accountant_accounts_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.accountant_accounts
    OWNER to personalassistant;

CREATE INDEX ix_accountant_accounts_user_id
    ON public.accountant_accounts USING btree
    (user_id)
    TABLESPACE pg_default;
