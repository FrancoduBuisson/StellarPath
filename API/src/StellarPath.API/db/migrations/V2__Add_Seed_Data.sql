INSERT INTO roles (role_name) VALUES
('Admin'),
('User');

INSERT INTO cruise_statuses (status_name) VALUES
('Scheduled'),
('In Progress'),
('Completed'),
('Cancelled');

INSERT INTO booking_statuses (status_name) VALUES
('Reserved'),
('Paid'),
('Completed'),
('Cancelled'),
('Expired');

INSERT INTO galaxies (galaxy_name, is_active) VALUES
('Milky Way', TRUE),
('Andromeda', TRUE),
('Triangulum', TRUE);

INSERT INTO star_systems (system_name, galaxy_id, is_active) VALUES
('Solar System', 1, TRUE),
('Alpha Centauri', 1, TRUE),
('Sirius', 1, TRUE),
('Epsilon Eridani', 1, TRUE);

INSERT INTO destinations (name, system_id, distance_from_earth, is_active) VALUES
('Earth', 1, 0, TRUE),
('Moon', 1, 384400, TRUE),
('Mars', 1, 225000000, TRUE),
('Venus', 1, 41400000, TRUE),
('Proxima Centauri b', 2, 40208000000, TRUE);

INSERT INTO ship_models (model_name, capacity, cruise_speed_kmph) VALUES
('Stellar Voyager I', 100, 50000),
('Galactic Explorer', 250, 75000),
('Cosmic Speedster', 50, 150000),
('Interstellar Liner', 500, 40000);