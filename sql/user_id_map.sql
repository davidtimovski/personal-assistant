CREATE TABLE public.user_id_map
(
    user_id integer NOT NULL,
    auth0_id character varying(50) NOT NULL COLLATE pg_catalog."default",
    CONSTRAINT pk_user_id_map PRIMARY KEY (user_id),
    CONSTRAINT fk_user_id_map_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.user_id_map
    OWNER to personalassistant;

CREATE INDEX ix_user_id_map_auth0_id
    ON public.user_id_map USING btree
    (auth0_id)
    TABLESPACE pg_default;
	