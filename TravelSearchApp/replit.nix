{ pkgs }:
pkgs.mkShell {
  buildInputs = [
    pkgs.docker
    pkgs.git
  ];
}
