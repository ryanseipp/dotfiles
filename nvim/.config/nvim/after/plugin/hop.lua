local g = require 'rs.globals'

g.nnoremap('<leader>hl', ':HopLine<CR>')
g.nnoremap('<leader>hw', ':HopWord<CR>')
g.nnoremap('<leader>hc', ':HopChar1<CR>')

require('hop').setup {
    keys = 'ashtneoiqdrwfup;gybjvkclzxm'
}
