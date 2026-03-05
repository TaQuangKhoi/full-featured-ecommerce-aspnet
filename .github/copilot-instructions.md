# GitHub Copilot Instructions

## JavaScript / Front-end conventions

- **Keep JavaScript out of Razor views.** Place all page-specific JavaScript in dedicated files under `src/ECommerce.Web/wwwroot/js/` (e.g., `cart.js`, `admin-edit-product.js`).
- Reference the file from the view's `@section Scripts` block:
  ```cshtml
  @section Scripts {
      @Html.AntiForgeryToken()
      <script src="~/js/my-feature.js"></script>
  }
  ```
- When a script needs server-generated values (e.g., URLs from `Url.Action`), pass them via `data-*` attributes on the relevant HTML element instead of embedding Razor expressions inside the JS file:
  ```cshtml
  <form id="my-form" data-index-url="@Url.Action("Index", "AdminProducts", new { area = "Admin" })">
  ```
  Then read them in JavaScript via `element.dataset.indexUrl`.

## Architecture

- **Clean Architecture**: ECommerce.Web (MVC + API controllers), ECommerce.Application (MediatR CQRS), ECommerce.Domain (Entities), ECommerce.Infrastructure (EF Core), ECommerce.Shared (DTOs).
- **API controllers** live in `src/ECommerce.Web/Controllers/Api/` and use `[ApiController]` + `[Route("api/...")]`.
- **Admin MVC controllers** live in `src/ECommerce.Web/Controllers/Admin/` and use `[Area("Admin")]` + `[Route("Admin/.../[action]")]`.

## Build & Test

- Build: `dotnet build ECommerce.slnx`
- Test: `dotnet test ECommerce.slnx`
