# UDP PoC

This is a small PoC of a UDP client-server with the server doing discovery via broadcasting IP

# Pre-requisites
- Node 12+ 

# Run
In different terminals you can run the following commands (you can use npm as well)
- pnpm start:client
- pnpm start:server


# Development/Debug
Run pnpm install so you can get the types for node and have a better dev experience (completely optional)
choose your the script you want to debug in vscode debug tab, you can also choose to run the compound (**Debug Client-Server**) which runs both scripts at the same time in debug mode, then press <kbd>F5</kbd> or the ***play*** button

For the moment Fable3 is not emitting source maps so you will have to debug the transpiled javascript (which is modern javascript so it is pretty readable)