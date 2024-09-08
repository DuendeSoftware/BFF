This project contains code that needs to be shared across Duende.Bff and
Duende.Bff.Blazor.Client. We can't depend on Duende.Bff in
Duende.Bff.Blazor.Client because the Duende.Bff has a framework reference to
aspnetcore and Duende.Bff.Blazor.Client is intended to be consumed in blazor 
wasm applications.

We can't depend on the Duende.Bff.Blazor.Client from Duende.Bff, because that
would bring all the blazor client work into the main package - we want that to
be opt in.