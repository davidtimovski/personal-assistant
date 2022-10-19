CREATE TABLE public.push_subscriptions
(
    id serial NOT NULL,
    user_id integer NOT NULL,
    application character varying(50) NOT NULL COLLATE pg_catalog."default",
    endpoint character varying(255) NOT NULL COLLATE pg_catalog."default",
    auth_key character varying(255) NOT NULL COLLATE pg_catalog."default",
    p256dh_key character varying(255) NOT NULL COLLATE pg_catalog."default",
    created_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_push_subscriptions PRIMARY KEY (id),
    CONSTRAINT fk_push_subscriptions_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.push_subscriptions
    OWNER to personalassistant;

CREATE INDEX ix_push_subscriptions_user_id
    ON public.push_subscriptions USING btree
    (user_id)
    TABLESPACE pg_default;
