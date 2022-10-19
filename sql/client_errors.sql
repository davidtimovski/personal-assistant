CREATE TABLE public.client_errors
(
    id serial NOT NULL,
    user_id integer NOT NULL,
	application character varying(20) NOT NULL COLLATE pg_catalog."default",
    message character varying(500) NOT NULL COLLATE pg_catalog."default",
	stack_trace character varying(10000) COLLATE pg_catalog."default",
    occurred timestamp with time zone NOT NULL,
    created_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_client_errors PRIMARY KEY (id),
    CONSTRAINT fk_client_errors_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.client_errors
    OWNER to personalassistant;

CREATE INDEX ix_client_errors_user_id
    ON public.client_errors USING btree
    (user_id)
    TABLESPACE pg_default;
