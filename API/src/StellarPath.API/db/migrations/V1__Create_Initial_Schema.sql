CREATE TABLE roles (
    role_id SERIAL PRIMARY KEY,
    role_name VARCHAR(50) NOT NULL
);

CREATE TABLE users (
    google_id VARCHAR(100) PRIMARY KEY,
    email VARCHAR(256) NOT NULL,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    role_id INT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY (role_id) REFERENCES roles(role_id)
);

CREATE TABLE galaxies (
    galaxy_id SERIAL PRIMARY KEY,
    galaxy_name VARCHAR(100) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE star_systems (
    system_id SERIAL PRIMARY KEY,
    system_name VARCHAR(100) NOT NULL,
    galaxy_id INT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY (galaxy_id) REFERENCES galaxies(galaxy_id)
);

CREATE TABLE destinations (
    destination_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    system_id INT NOT NULL,
    distance_from_earth BIGINT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY (system_id) REFERENCES star_systems(system_id)
);

CREATE TABLE ship_models (
    model_id SERIAL PRIMARY KEY,
    model_name VARCHAR(100) NOT NULL,
    capacity INT NOT NULL,
    cruise_speed_kmph INT NOT NULL
);

CREATE TABLE spaceships (
    spaceship_id SERIAL PRIMARY KEY,
    model_id INT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY (model_id) REFERENCES ship_models(model_id)
);

CREATE TABLE cruise_statuses (
    cruise_status_id SERIAL PRIMARY KEY,
    status_name VARCHAR(50) NOT NULL
);

CREATE TABLE cruises (
    cruise_id SERIAL PRIMARY KEY,
    spaceship_id INT NOT NULL,
    departure_destination_id INT NOT NULL,
    arrival_destination_id INT NOT NULL,
    local_departure_time TIMESTAMP NOT NULL,
    duration_minutes INT NOT NULL,
    cruise_seat_price NUMERIC NOT NULL,
    cruise_status_id INT NOT NULL,
    created_by_google_id VARCHAR(100) NOT NULL,
    FOREIGN KEY (spaceship_id) REFERENCES spaceships(spaceship_id),
    FOREIGN KEY (departure_destination_id) REFERENCES destinations(destination_id),
    FOREIGN KEY (arrival_destination_id) REFERENCES destinations(destination_id),
    FOREIGN KEY (cruise_status_id) REFERENCES cruise_statuses(cruise_status_id),
    FOREIGN KEY (created_by_google_id) REFERENCES users(google_id)
);

CREATE TABLE booking_statuses (
    booking_status_id SERIAL PRIMARY KEY,
    status_name VARCHAR(50) NOT NULL
);

CREATE TABLE bookings (
    booking_id SERIAL PRIMARY KEY,
    google_id VARCHAR(100) NOT NULL,
    cruise_id INT NOT NULL,
    seat_number INT NOT NULL,
    booking_date TIMESTAMP NOT NULL,
    booking_expiration TIMESTAMP NOT NULL,
    booking_status_id INT NOT NULL,
    FOREIGN KEY (google_id) REFERENCES users(google_id),
    FOREIGN KEY (cruise_id) REFERENCES cruises(cruise_id),
    FOREIGN KEY (booking_status_id) REFERENCES booking_statuses(booking_status_id),
    UNIQUE (cruise_id, seat_number)
);

CREATE TABLE booking_history (
    history_id SERIAL PRIMARY KEY,
    booking_id INT NOT NULL,
    previous_booking_status_id INT NOT NULL,
    new_booking_status_id INT NOT NULL,
    changed_at TIMESTAMP NOT NULL,
    FOREIGN KEY (booking_id) REFERENCES bookings(booking_id),
    FOREIGN KEY (previous_booking_status_id) REFERENCES booking_statuses(booking_status_id),
    FOREIGN KEY (new_booking_status_id) REFERENCES booking_statuses(booking_status_id)
);

CREATE INDEX idx_users_role_id ON users(role_id);
CREATE INDEX idx_star_systems_galaxy_id ON star_systems(galaxy_id);
CREATE INDEX idx_destinations_system_id ON destinations(system_id);
CREATE INDEX idx_spaceships_model_id ON spaceships(model_id);
CREATE INDEX idx_cruises_departure_destination_id ON cruises(departure_destination_id);
CREATE INDEX idx_cruises_arrival_destination_id ON cruises(arrival_destination_id);
CREATE INDEX idx_cruises_cruise_status_id ON cruises(cruise_status_id);
CREATE INDEX idx_cruises_created_by_google_id ON cruises(created_by_google_id);
CREATE INDEX idx_bookings_google_id ON bookings(google_id);
CREATE INDEX idx_bookings_cruise_id ON bookings(cruise_id);
CREATE INDEX idx_bookings_booking_status_id ON bookings(booking_status_id);
CREATE INDEX idx_booking_history_booking_id ON booking_history(booking_id);