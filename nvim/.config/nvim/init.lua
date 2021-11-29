if require 'rs.first_load'() then
  return
end

vim.g.mapleader = ' '

require 'rs.plugins'
require 'rs.lsp'
require 'rs.telescope.setup'
require 'rs.telescope.mappings'
