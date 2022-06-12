CREATE TABLE public.accountant_debts
(
    id serial NOT NULL,
    user_id integer NOT NULL,
    person character varying(20) COLLATE pg_catalog."default" NOT NULL,
    amount decimal(10, 2) NOT NULL,
    currency character varying(3) COLLATE pg_catalog."default" NOT NULL DEFAULT 'EUR',
    description character varying(255) COLLATE pg_catalog."default",
    user_is_debtor boolean NOT NULL DEFAULT FALSE,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_accountant_debts PRIMARY KEY (id),
    CONSTRAINT fk_accountant_debts_aspnetusers_user_id FOREIGN KEY (user_id)
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.accountant_debts
    OWNER to personalassistant;

CREATE INDEX ix_accountant_debts_user_id
    ON public.accountant_debts USING btree
    (user_id)
    TABLESPACE pg_default;
