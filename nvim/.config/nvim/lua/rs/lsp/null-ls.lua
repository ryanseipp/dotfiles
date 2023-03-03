local has_null, null_ls = pcall(require, 'null-ls')
if not has_null then
    return
end

local M = {}
M.setup = function(on_attach)
    null_ls.setup({
        sources = {
            null_ls.builtins.formatting.prettier.with({
                extra_filetypes = { "astro" },
            }),
            null_ls.builtins.formatting.csharpier,
        },
        on_attach = on_attach,
        debug = true,
    })
end

return M
