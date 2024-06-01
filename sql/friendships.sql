CREATE TABLE public.friendships
(
	sender_id integer NOT NULL,
	recipient_id integer NOT NULL,
    is_accepted boolean DEFAULT NULL,
	permissions text[] NOT NULL,
	created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_friendships PRIMARY KEY (sender_id, recipient_id),
    CONSTRAINT fk_friendships_users_sender_id FOREIGN KEY (sender_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_friendships_users_recipient_id FOREIGN KEY (recipient_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.friendships
    OWNER to personalassistant;

CREATE INDEX ix_friendships_sender_id
    ON public.friendships USING btree
    (sender_id)
    TABLESPACE pg_default;
    
CREATE INDEX ix_friendships_recipient_id
    ON public.friendships USING btree
    (recipient_id)
    TABLESPACE pg_default;
